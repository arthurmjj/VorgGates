using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VorgGates.App;
using VorgGates.Core;
using VorgGates.Gameplay;

namespace VorgGates.Tests
{
    public sealed class PrototypeFlowTests
    {
        private const int TestSeed = 4242;
        private const int OcclusionLayer = 31;

        private GameBootstrap game;
        private ScriptedPlayerInput input;

        private PlayerStateMachine Player => game.Player;
        private RoomController FirstCombat => game.Director.Current.Rooms.First(r => r.HasEncounter);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene("Game");
            yield return null;
            yield return WaitUntil(() =>
            {
                game = Object.FindAnyObjectByType<GameBootstrap>();
                return game != null && game.IsReady;
            }, 8f, "Game did not become ready");

            Assert.IsTrue(game.RestartWithSeed(TestSeed));
            yield return null;
            input = new ScriptedPlayerInput { AimPoint = Player.transform.position + Vector3.forward };
            Player.SetInput(input);
        }

        [UnityTest]
        public IEnumerator Aim_FacesCursor_WithinTwoDegrees()
        {
            yield return EnterFirstCombat();
            Player.Health.SetLocked(true);
            for (var i = 0; i < 8; i++)
            {
                var direction = Quaternion.Euler(0f, i * 45f + 7f, 0f) * Vector3.forward;
                input.AimPoint = Player.transform.position + direction * 5f;
                yield return null;
                yield return null;
                Assert.LessOrEqual(Vector3.Angle(Player.transform.forward, direction), 2f);
            }
        }

        [UnityTest]
        public IEnumerator Dodge_TravelsFiveMeters()
        {
            yield return EnterFirstCombat();
            Player.Health.SetLocked(true);
            var origin = FirstCombat.transform.position;
            Player.Motor.Teleport(origin + new Vector3(-2f, 0.05f, -2f), Quaternion.identity);
            var start = Player.transform.position;
            input.Move = new Vector2(0f, 1f);
            input.Dodge = true;
            yield return null;
            input.Move = Vector2.zero;
            yield return WaitUntil(() => !Player.Dodge.IsDodging, 1f, "Dodge did not finish");
            var moved = Player.transform.position - start;
            moved.y = 0f;
            Assert.AreEqual(5f, moved.magnitude, 0.3f);
        }

        [UnityTest]
        public IEnumerator Dodge_NeverPassesThroughWallsOrClosedDoors()
        {
            yield return EnterFirstCombat();
            Player.Health.SetLocked(true);
            var origin = FirstCombat.transform.position;
            for (var i = 0; i < 20; i++)
            {
                var direction = Quaternion.Euler(0f, i * 18f, 0f) * Vector3.forward;
                var start = origin + direction * 3f;
                start.y = 0.05f;
                Player.Dodge.ResetState();
                Player.Motor.Teleport(start, Quaternion.identity);
                yield return null;
                input.Move = WorldToInput(direction);
                input.Dodge = true;
                yield return null;
                yield return WaitUntil(() => !Player.Dodge.IsDodging, 1f, "Dodge did not finish");
                for (var f = 0; f < 6; f++) yield return null;
                input.Move = Vector2.zero;
                var local = Player.transform.position - origin;
                Assert.Less(Mathf.Abs(local.x), 9.1f, $"Attempt {i} crossed an X wall");
                Assert.Less(Mathf.Abs(local.z), 9.1f, $"Attempt {i} crossed a Z wall or door");
            }
        }

        [UnityTest]
        public IEnumerator Swing_DamagesTargetOnce()
        {
            yield return EnterFirstCombat();
            Player.Health.SetLocked(true);
            yield return WaitForEnemiesActive(FirstCombat);

            var enemy = FirstCombat.Enemies[0];
            var before = enemy.Health.Current;
            Player.Motor.Teleport(enemy.transform.position - Vector3.forward * 1.3f + Vector3.up * 0.05f, Quaternion.identity);
            input.AimPoint = enemy.transform.position;
            yield return null;
            input.AttackHeld = true;
            yield return null;
            input.AttackHeld = false;
            yield return WaitUntil(() => !Player.Weapon.IsAttacking, 1f, "Attack did not finish");
            Assert.AreEqual(before - Player.Weapon.Weapon.Damage, enemy.Health.Current);
        }

        [UnityTest]
        public IEnumerator Doors_CloseOnEntry_AndOpenOnlyWhenRoomIsClear()
        {
            yield return EnterFirstCombat();
            Player.Health.SetLocked(true);
            var room = FirstCombat;
            Assert.Greater(room.Doors.Count, 0);
            Assert.IsTrue(room.Doors.All(d => d.IsClosed));
            yield return WaitForEnemiesActive(room);

            var enemies = room.Enemies.ToArray();
            Assert.AreEqual(room.EnemyCount, enemies.Length);
            for (var i = 0; i < enemies.Length; i++)
            {
                Assert.IsTrue(room.Doors.All(d => d.IsClosed), $"Doors opened with {enemies.Length - i} enemies alive");
                enemies[i].Health.ApplyDamage(new DamageInfo(999, Vector3.forward, 0f));
                yield return null;
            }

            Assert.AreEqual(RoomState.Cleared, room.State);
            Assert.IsTrue(room.Doors.All(d => !d.IsClosed));
        }

        [UnityTest]
        public IEnumerator Enemies_CannotLeaveThroughClosedDoors()
        {
            yield return EnterFirstCombat();
            Player.Health.SetLocked(true);
            var room = FirstCombat;
            yield return WaitForEnemiesActive(room);
            yield return new WaitForSeconds(0.2f);

            var outside = room.transform.position + new Vector3(0f, 0f, 0f);
            foreach (var door in room.Doors)
            {
                var doorPoint = door.transform.position;
                var beyond = doorPoint + (doorPoint - room.transform.position).normalized * 3f;
                Assert.IsTrue(NavMesh.SamplePosition(outside, out var from, 2f, NavMesh.AllAreas));
                Assert.IsTrue(NavMesh.SamplePosition(beyond, out var to, 2f, NavMesh.AllAreas));
                var path = new NavMeshPath();
                NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, path);
                Assert.AreNotEqual(NavMeshPathStatus.PathComplete, path.status, "A closed door must cut the NavMesh");
            }
        }

        [UnityTest]
        public IEnumerator Room_CannotBeBypassedAlongTheWalls()
        {
            var room = FirstCombat;
            var origin = room.transform.position;
            foreach (var offset in new[]
                     {
                         new Vector3(-8.5f, 0.05f, -8.5f), new Vector3(8.5f, 0.05f, -4f),
                         new Vector3(-8.5f, 0.05f, 6.2f), new Vector3(5.2f, 0.05f, 8.5f)
                     })
            {
                room.ResetRoom();
                Player.Motor.Teleport(origin + offset, Quaternion.identity);
                yield return WaitUntil(() => room.State == RoomState.Active, 1f, $"Room did not activate at {offset}");
            }
        }

        [UnityTest]
        public IEnumerator BuiltLayouts_HaveValidNavigationAndContent()
        {
            for (var seed = 100; seed < 130; seed++)
            {
                Assert.IsTrue(game.RestartWithSeed(seed));
                yield return null;
                Player.SetControlsEnabled(false);
                var layout = game.Director.Layout;
                var built = game.Director.Current;

                Assert.IsNotNull(built.Portal, $"Seed {seed}: no portal");
                Assert.IsFalse(built.Portal.IsAvailable, $"Seed {seed}: portal starts active");
                Assert.IsTrue(NavMesh.SamplePosition(built.PlayerStart.position, out var start, 1f, NavMesh.AllAreas), $"Seed {seed}: start off NavMesh");

                foreach (var node in layout.Rooms)
                {
                    var room = built.Rooms[node.Id];
                    Assert.AreEqual(node.Kind == RoomKind.Combat || node.Kind == RoomKind.Boss, room.HasEncounter, $"Seed {seed}, room {node.Id}");
                    var target = DungeonBuilder.ToWorld(node.Position) + (node.Kind == RoomKind.Reward ? new Vector3(0f, 0f, -2f) : Vector3.zero);
                    AssertPath(start.position, target, $"Seed {seed}: room {node.Id} unreachable");
                    if (!room.HasEncounter) continue;

                    Assert.AreEqual(BitCount(node.DoorMask), room.Doors.Count, $"Seed {seed}, room {node.Id}: wrong door count");
                    foreach (var spawn in room.SpawnPoints)
                    {
                        Assert.IsTrue(NavMesh.SamplePosition(spawn.position, out var hit, 0.6f, NavMesh.AllAreas), $"Seed {seed}: spawn {spawn.position} off NavMesh");
                        Assert.Less(Vector3.Distance(hit.position, spawn.position), 0.6f);
                    }
                }

                AssertPath(start.position, built.Portal.transform.position + new Vector3(0f, 0f, -2f), $"Seed {seed}: portal unreachable");
            }
        }

        [UnityTest]
        public IEnumerator Camera_NeverHidesThePlayer_InRoomsOrCorridors()
        {
            foreach (var (phase, seed) in new[] { (0, 7), (0, 88), (1, 321), (1, 4242), (2, 9001), (2, 55) })
            {
                Assert.IsTrue(game.JumpToDungeon(phase, seed));
                yield return null;
                Player.SetControlsEnabled(false);
                var built = game.Director.Current;
                var added = new List<MeshCollider>();
                foreach (var renderer in built.Root.GetComponentsInChildren<MeshRenderer>())
                {
                    if (renderer.GetComponentInParent<DoorController>() != null || renderer.GetComponentInParent<PrototypePortal>() != null || renderer.GetComponentInParent<WeaponPickup>() != null || renderer.GetComponentInParent<LegendarySword>() != null) continue;
                    var filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null || filter.sharedMesh == null || !filter.sharedMesh.isReadable) continue;
                    var meshCollider = renderer.gameObject.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = filter.sharedMesh;
                    renderer.gameObject.layer = OcclusionLayer;
                    added.Add(meshCollider);
                }

                Assert.Greater(added.Count, 20, "No occluder geometry was found");
                Physics.SyncTransforms();
                var toCamera = -Camera.main.transform.forward;
                var bounds = built.Bounds;
                var samples = 0;
                var hidden = new List<Vector3>();
                for (var x = bounds.min.x; x <= bounds.max.x; x += 0.75f)
                for (var z = bounds.min.z; z <= bounds.max.z; z += 0.75f)
                {
                    var point = new Vector3(x, 0.5f, z);
                    if (!NavMesh.SamplePosition(point, out var hit, 0.55f, NavMesh.AllAreas)) continue;
                    if (Mathf.Abs(hit.position.x - x) > 0.05f || Mathf.Abs(hit.position.z - z) > 0.05f) continue;
                    samples++;
                    var chest = hit.position + Vector3.up * 1.05f;
                    if (Physics.Raycast(chest, toCamera, 45f, 1 << OcclusionLayer, QueryTriggerInteraction.Ignore)) hidden.Add(hit.position);
                }

                Assert.Greater(samples, 500, "Too few walkable samples");
                Assert.AreEqual(0, hidden.Count, $"Seed {seed}: {hidden.Count}/{samples} hidden, first at {(hidden.Count > 0 ? hidden[0] : Vector3.zero)}");
                foreach (var meshCollider in added) Object.Destroy(meshCollider);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator Death_RebuildsNewLayoutWithinOneSecond_TenTimes()
        {
            var previous = game.Director.Layout.Signature;
            for (var i = 0; i < 10; i++)
            {
                yield return null;
                Player.Health.ApplyDamage(new DamageInfo(999, Vector3.forward, 0f));
                Assert.AreEqual(GameFlowState.Dying, game.Flow.State);
                var diedAt = Time.realtimeSinceStartup;
                yield return WaitUntil(() => game.Flow.State == GameFlowState.Playing, 2f, "Dungeon did not restart");
                Assert.LessOrEqual(Time.realtimeSinceStartup - diedAt, 1f);
                Assert.AreEqual(100, Player.Health.Current);
                Assert.AreNotEqual(previous, game.Director.Layout.Signature, "Death must change the layout");
                previous = game.Director.Layout.Signature;
                Assert.Less(Vector3.Distance(Player.transform.position, game.Director.Current.PlayerStart.position), 0.25f);
                yield return null;
                yield return null;
                var view = Camera.main.WorldToViewportPoint(Player.transform.position);
                Assert.Less(Mathf.Abs(view.x - 0.5f), 0.1f, "Camera must snap to the player after a restart");
                Assert.Less(Mathf.Abs(view.y - 0.5f), 0.12f, "Camera must snap to the player after a restart");
            }
        }

        [UnityTest]
        public IEnumerator Rebuilds_DoNotLeakObjects()
        {
            Assert.IsTrue(game.RestartWithSeed(55));
            yield return null;
            yield return null;
            var baseline = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Length;
            for (var i = 0; i < 50; i++)
            {
                Assert.IsTrue(game.RestartWithSeed(55));
                yield return null;
            }

            yield return null;
            Assert.AreEqual(baseline, Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Length);
        }

        [UnityTest]
        public IEnumerator RealBindings_ReachPlayerInput()
        {
            var reader = Player.GetComponent<PlayerInputReader>();

            // An unfocused Editor disables input devices, so the simulated ones must be kept enabled.
            var settings = InputSystem.settings;
            var previousBackground = settings.backgroundBehavior;
            var previousEditorBehavior = settings.editorInputBehaviorInPlayMode;
            settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var mouse = InputSystem.AddDevice<Mouse>();
            InputSystem.EnableDevice(keyboard);
            InputSystem.EnableDevice(mouse);
            try
            {
                // Actions re-resolve their bindings after a device is added, which completes on a later update.
                yield return null;
                yield return null;

                foreach (var (key, expected) in new[]
                         {
                             (Key.W, Vector2.up), (Key.S, Vector2.down), (Key.A, Vector2.left), (Key.D, Vector2.right)
                         })
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
                    var waited = 0;
                    while ((reader.Move - expected).magnitude >= 0.01f && waited < 30)
                    {
                        waited++;
                        yield return null;
                    }

                    Assert.Less((reader.Move - expected).magnitude, 0.01f,
                        $"{key} binding; keyPressed={keyboard[key].isPressed}; deviceEnabled={keyboard.enabled}; focused={Application.isFocused}");
                }

                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
                InputSystem.Update();
                Assert.IsTrue(reader.ConsumeDodge(), "Space binding");

                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                InputSystem.Update();
                Assert.IsTrue(reader.ConsumeInteract(), "E binding");

                foreach (var (key, slot) in new[] { (Key.Digit1, 0), (Key.Digit2, 1), (Key.Digit3, 2) })
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    InputSystem.Update();
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
                    InputSystem.Update();
                    Assert.AreEqual(slot, reader.ConsumeWeaponSlot(), $"{key} binding");
                }

                var screenPoint = new Vector2(Screen.width * 0.7f, Screen.height * 0.4f);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = screenPoint }.WithButton(MouseButton.Left));
                InputSystem.Update();
                Assert.IsTrue(reader.AttackHeld, "Left mouse binding");
                var projected = (Vector2)Camera.main.WorldToScreenPoint(reader.AimPoint);
                Assert.Less((projected - screenPoint).magnitude, 1f, "Mouse aim must land under the cursor");
            }
            finally
            {
                settings.backgroundBehavior = previousBackground;
                settings.editorInputBehaviorInPlayMode = previousEditorBehavior;
                InputSystem.RemoveDevice(keyboard);
                InputSystem.RemoveDevice(mouse);
            }

            yield return null;
        }

        private IEnumerator EnterFirstCombat()
        {
            var room = FirstCombat;
            Player.Motor.Teleport(room.transform.position + Vector3.up * 0.05f, Quaternion.identity);
            yield return WaitUntil(() => room.State == RoomState.Active, 1f, "Room did not activate");
        }

        private static IEnumerator WaitForEnemiesActive(RoomController room)
        {
            yield return WaitUntil(() => room.Enemies.Count == room.EnemyCount && room.Enemies.All(e => e == null || e.State != EnemyState.Dormant), 2f, "Enemies did not activate");
        }

        private static void AssertPath(Vector3 from, Vector3 target, string message)
        {
            Assert.IsTrue(NavMesh.SamplePosition(target, out var to, 2f, NavMesh.AllAreas), message);
            var path = new NavMeshPath();
            Assert.IsTrue(NavMesh.CalculatePath(from, to.position, NavMesh.AllAreas, path), message);
            Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status, message);
        }

        private static int BitCount(int mask)
        {
            var count = 0;
            for (; mask != 0; mask &= mask - 1) count++;
            return count;
        }

        private static Vector2 WorldToInput(Vector3 direction)
        {
            var right = IsoMovement.ToWorld(Vector2.right, 45f);
            var up = IsoMovement.ToWorld(Vector2.up, 45f);
            return new Vector2(Vector3.Dot(direction, right), Vector3.Dot(direction, up));
        }

        private static IEnumerator WaitUntil(System.Func<bool> condition, float timeout, string message)
        {
            var deadline = Time.realtimeSinceStartup + timeout;
            while (!condition())
            {
                Assert.Less(Time.realtimeSinceStartup, deadline, message);
                yield return null;
            }
        }
    }
}
