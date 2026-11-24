# Vorg'Gates

> Três portões. Uma lâmina na pedra.

Vorg'Gates é um jogo 3D de exploração e combate em masmorras, com estética low poly e câmera isométrica, desenvolvido em Unity para Windows. O jogador controla Vorg, herdeiro de um antigo rei, que atravessa três masmorras geradas proceduralmente e enfrenta três chefões para retirar a lendária Lâmina de Vorg da pedra.

Projeto acadêmico do curso de Ciência da Computação da Universidade de Marília (UNIMAR).

| | |
|---|---|
| **Desenvolvedores** | Arthur Martins Jorge e Gabriel De Castro Domingo |
| **Gênero** | Aventura, exploração e combate em masmorras 3D |
| **Estilo** | Dungeon crawler de fantasia medieval, low poly, câmera isométrica |
| **Inspirações** | Lenda do Rei Arthur e Soul Knight |
| **Plataforma** | Windows 10 ou 11 de 64 bits, mouse e teclado |
| **Motor** | Unity 6000.6.10f1 (Unity 6.6) com Universal Render Pipeline |

## História

O Rei-Feiticeiro Vorg fundou o reino de Valdren com uma espada forjada em luz dourada. Antes de morrer, cravou a lâmina em uma pedra e selou três portões, determinando que ela só pertenceria a quem provasse ser digno, e não apenas herdeiro.

Vorg, seu descendente, atravessa as três provações: a coragem na **Cripta de Musgo**, a perseverança na **Forja Afundada** e a legitimidade no **Santuário de Cinzas**. As masmorras se reorganizam a cada tentativa. Ao vencer os três portões, ele chega à Câmara da Espada para concluir a jornada.

A história completa está em [Docs/HISTORIA.md](Docs/HISTORIA.md).

## Funcionalidades

- Três fases com arquiteturas, inimigos e chefões próprios.
- Geração procedural de masmorras, com validação e um novo layout a cada tentativa.
- Combate em tempo real com Espada, Machado e Cajado.
- Três tipos de inimigos e três chefões com padrões de ataque sempre anunciados no chão.
- Checkpoint por fase: ao morrer, apenas a masmorra atual recomeça.
- Menu, pausa, HUD, cartões de transição e tela de vitória.
- Trilhas e efeitos sonoros sintetizados no próprio projeto.

## Controles

| Ação | Entrada |
|---|---|
| Mover | WASD |
| Mirar | Mouse |
| Atacar | Clique esquerdo |
| Esquivar | Espaço |
| Interagir | E |
| Trocar arma | 1, 2, 3 ou roda do mouse |
| Pausar | Esc |

## Tecnologias

- Unity 6000.6.10f1 e Universal Render Pipeline 17.6.0
- C# com back-end Mono
- Input System 1.20.1, Cinemachine 6.6.0 e UI Toolkit
- Unity Test Framework 1.8.0
- Git e Git LFS

## Como abrir o projeto

1. Instale o Unity Hub e o **Unity 6000.6.10f1**, com o módulo Windows Build Support.
2. Clone o repositório com Git LFS instalado, para baixar texturas, modelos, áudio e fontes:

   ```powershell
   git lfs install
   git clone <url-do-repositorio> VorgGates
   ```

3. No Unity Hub, clique em **Add** e selecione a pasta clonada.
4. Abra o projeto. A primeira abertura importa os assets e pode levar alguns minutos.
5. Abra a cena `Assets/_Project/Scenes/Game.unity` e pressione **Play**.

Para gerar o executável, use o menu **Vorg'Gates > Build Windows Release**. A build é criada em `Builds/Release/Windows`.

## Estrutura

```text
Assets/_Project/
├── Art/            Materiais, shaders, texturas, malhas e assets externos
├── Audio/          Trilhas e efeitos sonoros
├── Data/           Armas, inimigos, fases e atmosferas (ScriptableObjects)
├── Prefabs/        Jogador, inimigos, chefões, salas e objetos
├── Scenes/         Game.unity
├── Scripts/
│   ├── Core/          Regras em C# puro: sessão, checkpoints, gerador e validador
│   ├── Gameplay/      Jogador, combate, inimigos, salas e construção da masmorra
│   ├── Presentation/  Interface, áudio, efeitos e animações visuais
│   ├── App/           Composição e fluxo do jogo
│   └── Editor/        Construção do conteúdo e builds
└── UI/             UXML, USS, fontes e ícones
Assets/Tests/       Testes EditMode, PlayMode e de build
Docs/               Documentação do projeto
```

As regras de jogo ficam em `Core`, sem dependência do Unity. A camada `Presentation` apenas reage ao estado do jogo, o que permite alterar a parte visual sem mudar a gameplay. O conteúdo das salas e personagens é gerado pelo menu **Vorg'Gates > Rebuild Game**, e os prefabs gerados já estão versionados.

## Testes

Os testes são executados em **Window > General > Test Runner**:

- **EditMode:** regras de checkpoint, inventário, combate e geração procedural, incluindo 1.000 sementes por configuração.
- **PlayMode:** fluxo do jogo, portas, combate, chefões, interface e a campanha completa jogada por um robô.

Na versão final, 45 testes EditMode e 36 testes PlayMode foram aprovados.

## Documentação

- [Documentação acadêmica](Docs/DOCUMENTACAO_ACADEMICA.md)
- [História e universo](Docs/HISTORIA.md)
- [Roteiro do trailer](Docs/TRAILER.md)
- [Créditos e licenças](Docs/ASSET_CREDITS.md)

## Ferramentas de apoio

O desenvolvimento contou com o assistente de programação Claude Code (Anthropic), usado sob direção e aprovação dos alunos para implementação de código, geração de conteúdo procedural, automação de testes e builds e redação de documentação. As decisões de design, a aprovação de cada etapa e os testes manuais de jogo foram feitos pelos alunos.

## Créditos e licenças

Trilhas, efeitos sonoros, interface, o personagem principal, os inimigos comuns e o Guardião da Cripta foram criados para o projeto. Os assets de terceiros usados são:

| Asset | Autor | Licença |
|---|---|---|
| [Undead Knight [Low-Poly]](https://sketchfab.com/3d-models/undead-knight-low-poly-0453f93869af4776ac69d9ef896aa6a1) | SchrisArt | CC BY 4.0 |
| [Modular Dungeon Kit](https://sigilsvault.itch.io/modular-dungeon-kit-v10) e [Modular Cathedral Kit](https://sigilsvault.itch.io/modular-cathedral-kit) | Kevin Barany / SigilsVault | CC0 1.0 |
| [Seahill Dungeon Complete](https://seehiong.itch.io/seahill-dungeons) | See Hiong / Seahill Studio | CC0 1.0 |
| [Fullplate Armor Knight](https://magicalgothgirrl.itch.io/fullplate-armor-knight) | Luana Coppio | CC0 1.0 |
| Foto "Suit Of Armor" | Petr Kratochvil | Domínio público |
| Graveyard Kit, Mini Dungeon, Nature Kit e Particle Pack | Kenney | CC0 1.0 |
| Fonte Cinzel | The Cinzel Project Authors | SIL Open Font License 1.1 |

**Atribuição obrigatória (CC BY 4.0):**

This work is based on "Undead Knight [Low-Poly]" (https://sketchfab.com/3d-models/undead-knight-low-poly-0453f93869af4776ac69d9ef896aa6a1) by SchrisArt (https://sketchfab.com/SchrisArt) licensed under CC-BY-4.0 (http://creativecommons.org/licenses/by/4.0/). O modelo foi modificado: malha convertida, flechas removidas, materiais recoloridos, esqueleto e animações criados para o jogo.

Os detalhes de cada asset e suas alterações estão em [Docs/ASSET_CREDITS.md](Docs/ASSET_CREDITS.md). Os arquivos de licença originais acompanham os assets em `Assets/_Project/Art/External`.
