# Vorg'Gates — Créditos e Licenças

Versão final, 24/11/2026.

Desenvolvido por Arthur Martins Jorge e Gabriel De Castro Domingo, alunos de Ciência da Computação da Universidade de Marília (UNIMAR).

## Assets de terceiros

| Asset | Autor | Origem | Licença | Uso no jogo |
|---|---|---|---|---|
| Undead Knight [Low-Poly] | [SchrisArt](https://sketchfab.com/SchrisArt) | [Sketchfab](https://sketchfab.com/3d-models/undead-knight-low-poly-0453f93869af4776ac69d9ef896aa6a1) | [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/) | Carrasco da Forja. **Atribuição obrigatória** |
| Modular Dungeon Kit v1.0 (seleção) | Kevin Barany / SigilsVault | [itch.io](https://sigilsvault.itch.io/modular-dungeon-kit-v10) | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) | Arquitetura da Cripta, pisos da Forja e sarcófagos |
| Modular Cathedral Kit v1.0 (seleção) | Kevin Barany / SigilsVault | [itch.io](https://sigilsvault.itch.io/modular-cathedral-kit) | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) | Arquitetura do Santuário |
| Seahill Dungeon Complete v1.0.0 (seleção) | See Hiong / Seahill Studio | [itch.io](https://seehiong.itch.io/seahill-dungeons) | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) | Estruturas metálicas e grades da Forja |
| Fullplate Armor Knight | Luana Coppio | [itch.io](https://magicalgothgirrl.itch.io/fullplate-armor-knight) | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) | Sentinela de Vorg |
| Foto "Suit Of Armor" | Petr Kratochvil | [PublicDomainPictures](https://www.publicdomainpictures.net/en/view-image.php?image=377696&picture=suit-of-armor) | Domínio público | Base da textura do Fullplate Armor Knight |
| Graveyard Kit (seleção) | Kenney | [kenney.nl](https://kenney.nl/assets/graveyard-kit) | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) | Lápides, colunas, urnas, velas e detritos |
| Mini Dungeon (seleção) | Kenney | [kenney.nl](https://kenney.nl/assets/mini-dungeon) | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) | Baú, elixir, barris, potes e pedras |
| Nature Kit (seleção) | Kenney | [kenney.nl](https://kenney.nl/assets/nature-kit) | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) | Vegetação, cogumelos e pedras |
| Particle Pack (seleção) | Kenney | [kenney.nl](https://kenney.nl/assets/particle-pack) | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) | Faíscas, magia e névoa |
| Cinzel Bold | The Cinzel Project Authors | [Google Fonts](https://fonts.google.com/specimen/Cinzel), [projeto](https://github.com/NDISCOVER/Cinzel) | [SIL Open Font License 1.1](https://openfontlicense.org) | Título e títulos principais |

### Atribuição obrigatória (CC BY 4.0)

This work is based on "Undead Knight [Low-Poly]" (https://sketchfab.com/3d-models/undead-knight-low-poly-0453f93869af4776ac69d9ef896aa6a1) by SchrisArt (https://sketchfab.com/SchrisArt) licensed under CC-BY-4.0 (http://creativecommons.org/licenses/by/4.0/).

**Alterações realizadas:** malha convertida de glTF para OBJ, flechas removidas, texturas reduzidas, armadura escurecida, capa recolorida, áreas incandescentes adicionadas, esqueleto e animações procedurais criados para o jogo. O autor original não endossa este projeto nem estas alterações.

### Alterações nos demais assets

- **Kits da SigilsVault e Seahill:** apenas as peças necessárias foram importadas. As peças foram reescaladas para as dimensões das salas, receberam materiais de cada fase e não têm colisão. Os modelos Seahill foram convertidos de GLB para OBJ.
- **Fullplate Armor Knight:** proporções ampliadas, material violeta e uso do rig e das animações originais de repouso e caminhada.
- **Kenney:** modelos usados como decoração, com materiais URP próprios e sem colisão.
- **Cinzel:** fonte usada sem modificação. O texto da licença acompanha o jogo.

Os arquivos de licença originais estão em `Assets/_Project/Art/External` e acompanham a distribuição na pasta `Licencas`.

## Conteúdo criado para o projeto

| Conteúdo | Como foi criado |
|---|---|
| Vorg, inimigos comuns e Guardião da Cripta | Modelos low poly montados por código no Unity |
| Portas, portais, armas, altar e objetos de cenário | Modelos low poly montados por código no Unity |
| 20 efeitos sonoros e 3 trilhas | Sintetizados por código no Unity |
| Interface e ícones das armas | UI Toolkit, UXML, USS e ícones gerados por código |
| Texturas de pedra, piso, madeira e metal | Geradas por código |
| Lava, runas, cristais e portais animados | Shader próprio para URP |
| Esqueleto e animações do Carrasco da Forja | Criados para o modelo Undead Knight |

## Tecnologia de terceiros

| Item | Autor | Uso |
|---|---|---|
| Unity 6.6 e Universal Render Pipeline | Unity Technologies | Motor e renderização |
| Input System, Cinemachine e UI Toolkit | Unity Technologies | Controles, câmera e interface |
| Fonte padrão do tema de UI do Unity | Distribuída com o Unity | Botões, HUD e textos pequenos |

Esses componentes são usados conforme os termos do Unity aceitos para este Editor.
