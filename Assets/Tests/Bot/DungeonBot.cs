using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using VorgGates.App;
using VorgGates.Core;
using VorgGates.Gameplay;

namespace VorgGates.Tests
{
    /// <summary>Plays a dungeon through the real player input: explores every room, fights and uses the portal.</summary>
    public sealed class DungeonBot : IPlayerInput
    {
        private const float RepathInterval = 0.2f;
        private const float RoomReachedDistance = 2.5f;
        private const float PortalDistance = 2.45f;
        private const float DodgeWindow = 0.16f;
        private const int WorldMask = 1 << 6;

        private readonly GameBootstrap game;
        private readonly NavMeshPath path = new NavMeshPath();
        private bool dodgeRequest;
        private bool interactRequest;
        private int corner;
        private float nextRepath;

        public DungeonBot(GameBootstrap gameBootstrap) => game = gameBootstrap;

        public Vector2 Move { get; private set; }
        public Vector3 AimPoint { get; private set; }
        public bool AttackHeld { get; private set; }
        public string Failure { get; private set; }
        public int RoomsVisited { get; private set; }
        public int EnemiesDefeated { get; private set; }
        public bool ReachedExit { get; private set; }

        public bool ConsumeDodge()
        {
            var value = dodgeRequest;
            dodgeRequest = false;
            return value;
        }

        public bool ConsumeInteract()
        {
            var value = interactRequest;
            interactRequest = false;
            return value;
        }

        public bool ConsumePause() => false;

        public int ConsumeWeaponSlot() => -1;

        public int ConsumeWeaponCycle() => 0;

        public void RequestInteract() => interactRequest = true;

        public IEnumerator CompleteDungeon(float timeoutSeconds, bool invulnerable = true)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            var player = game.Player;
            var director = game.Director;
            var layout = director.Layout;
            var built = director.Current;
            var exit = built.Portal != null ? (IInteractable)built.Portal : built.Sword;
            var exitTransform = built.Portal != null ? built.Portal.transform : built.Sword.transform;
            var pending = layout.Rooms.Where(r => r.Kind != RoomKind.Portal && r.Kind != RoomKind.SwordChamber).OrderBy(r => r.Depth).ToList();
            pending.RemoveAll(r => r.Kind == RoomKind.Entrance);
            player.SetInput(this);
            player.Health.SetLocked(invulnerable);
            RoomsVisited = 1;

            while (game.Flow.State == GameFlowState.Playing)
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    Failure = $"Timed out at {player.transform.position}; seed {layout.Seed}; rooms left {pending.Count}; combat left {director.CombatRoomsRemaining}";
                    break;
                }

                var active = director.ActiveRoom;
                if (active != null)
                {
                    Fight(active);
                }
                else
                {
                    AttackHeld = false;
                    if (built.Reward != null && !built.Reward.IsCollected && ReferenceEquals(player.Interactor.Current, built.Reward)) RequestInteract();
                    pending.RemoveAll(r =>
                    {
                        var reached = FlatDistance(player.transform.position, DungeonBuilder.ToWorld(r.Position)) < RoomReachedDistance
                                      && (!built.Rooms[r.Id].HasEncounter || built.Rooms[r.Id].State == RoomState.Cleared)
                                      && (r.Kind != RoomKind.Reward || built.Reward.IsCollected);
                        if (reached) RoomsVisited++;
                        return reached;
                    });

                    var destination = pending.Count > 0 ? NextDestination(pending, built) : exitTransform.position;
                    if (pending.Count == 0 && exit.CanInteract && FlatDistance(player.transform.position, destination) < PortalDistance)
                    {
                        Move = Vector2.zero;
                        if (ReferenceEquals(player.Interactor.Current, exit)) RequestInteract();
                    }
                    else
                    {
                        Navigate(destination);
                    }
                }

                yield return null;
            }

            Move = Vector2.zero;
            AttackHeld = false;
            ReachedExit = game.Flow.State == GameFlowState.Victory || game.Flow.State == GameFlowState.Transition;
            if (!ReachedExit && Failure == null) Failure = $"Ended in state {game.Flow.State}; seed {layout.Seed}";
        }

        private static Vector3 NextDestination(List<RoomNode> pending, BuiltDungeon built)
        {
            var next = pending.FirstOrDefault(r => built.Rooms[r.Id].HasEncounter && built.Rooms[r.Id].State != RoomState.Cleared) ?? pending[0];
            return DungeonBuilder.ToWorld(next.Position) + OpenSpotOffset(next);
        }

        // Reward rooms have a pedestal at the center, so the bot stands beside it.
        private static Vector3 OpenSpotOffset(RoomNode room) => room.Kind == RoomKind.Reward ? new Vector3(0f, 0f, -1.8f) : Vector3.zero;

        private void Fight(RoomController room)
        {
            var player = game.Player.transform;
            EnemyBrain target = null;
            var best = float.MaxValue;
            foreach (var enemy in room.Enemies)
            {
                if (enemy == null || enemy.Health.IsDead) continue;
                var distance = (enemy.transform.position - player.position).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                target = enemy;
            }

            if (target == null)
            {
                Move = Vector2.zero;
                AttackHeld = false;
                return;
            }

            AimPoint = target.transform.position;
            var range = Mathf.Sqrt(best);
            var weapon = game.Player.Weapon.Weapon;
            var sightHeight = weapon.IsRanged ? 1.15f : 1.3f;
            var clearShot = !Physics.Linecast(player.position + Vector3.up * sightHeight, target.transform.position + Vector3.up * sightHeight, WorldMask, QueryTriggerInteraction.Ignore);
            AttackHeld = clearShot && range < weapon.Range + 0.2f;
            var holdDistance = weapon.IsRanged ? weapon.Range * 0.6f : 1.7f;
            if (range > holdDistance || !clearShot) Navigate(target.transform.position);
            else Move = Vector2.zero;

            // Dodges a telegraphed attack at the last moment, using the same input as a player.
            var threat = FindImminentThreat(room, player);
            if (threat != null)
            {
                var away = player.position - threat.transform.position;
                away.y = 0f;
                var side = Vector3.Cross(Vector3.up, away.sqrMagnitude > 0.01f ? away.normalized : player.forward);
                var right = IsoMovement.ToWorld(Vector2.right, 45f);
                var up = IsoMovement.ToWorld(Vector2.up, 45f);
                Move = new Vector2(Vector3.Dot(side, right), Vector3.Dot(side, up));
                dodgeRequest = true;
            }
        }

        private static EnemyBrain FindImminentThreat(RoomController room, Transform player)
        {
            foreach (var enemy in room.Enemies)
            {
                if (enemy == null || enemy.State != EnemyState.Telegraph || enemy.TelegraphRemaining >= DodgeWindow) continue;
                if (enemy.Definition.Behavior == EnemyBehavior.Ranged) continue;
                var reach = enemy.IsBoss ? 16f : enemy.Definition.Behavior == EnemyBehavior.Charger ? 9f : 3.5f;
                if (FlatDistance(player.position, enemy.transform.position) <= reach) return enemy;
            }

            return null;
        }

        private void Navigate(Vector3 destination)
        {
            var player = game.Player.transform;
            if (Time.time >= nextRepath)
            {
                nextRepath = Time.time + RepathInterval;
                if (NavMesh.SamplePosition(player.position, out var from, 1.5f, NavMesh.AllAreas)
                    && NavMesh.SamplePosition(destination, out var to, 2f, NavMesh.AllAreas))
                {
                    NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, path);
                    corner = 1;
                }
            }

            var corners = path.corners;
            while (corner < corners.Length - 1 && FlatDistance(player.position, corners[corner]) < 0.4f) corner++;
            var waypoint = corner < corners.Length ? corners[corner] : destination;
            var delta = waypoint - player.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < 0.01f)
            {
                Move = Vector2.zero;
                return;
            }

            var direction = delta.normalized;
            if (!AttackHeld) AimPoint = player.position + direction * 3f;
            var right = IsoMovement.ToWorld(Vector2.right, 45f);
            var up = IsoMovement.ToWorld(Vector2.up, 45f);
            Move = new Vector2(Vector3.Dot(direction, right), Vector3.Dot(direction, up));
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
