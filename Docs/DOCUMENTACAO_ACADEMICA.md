# Vorg'Gates — Documentação do Projeto

## Identificação

| Campo | Informação |
|---|---|
| Instituição | Universidade de Marília (UNIMAR) |
| Curso | Ciência da Computação |
| Projeto | Vorg'Gates |
| Versão | 1.2.0 (versão final, tag `v1.2.0-final`) |
| Data | 24/11/2026 |

| Alunos |
|---|
| Arthur Martins Jorge |
| Gabriel De Castro Domingo |

---

## 1. Introdução

Vorg'Gates é um jogo 3D de exploração e combate em masmorras, desenvolvido em Unity para Windows. O projeto reúne, em uma aplicação completa e jogável do início ao fim, tópicos de Ciência da Computação como geração procedural de conteúdo, máquinas de estado, inteligência artificial de inimigos, navegação em malha, arquitetura em camadas e testes automatizados.

## 2. Apresentação do jogo

O jogador controla Vorg, herdeiro de um antigo rei, que atravessa três masmorras para retirar a Lâmina de Vorg de uma pedra. Cada masmorra é gerada novamente a cada tentativa e termina com um chefão. A campanha completa leva cerca de 20 a 30 minutos.

- **Gênero:** aventura, exploração e combate em masmorras 3D.
- **Estilo:** dungeon crawler de fantasia medieval, low poly, com câmera isométrica.
- **Plataforma:** Windows de 64 bits, com mouse e teclado.
- **Jogadores:** um.
- **Slogan:** Três portões. Uma lâmina na pedra.

## 3. Objetivo do projeto

Desenvolver um jogo completo e distribuível que aplique técnicas de programação e engenharia de software em um contexto prático. Os objetivos específicos foram:

- Implementar geração procedural de masmorras com garantias de jogabilidade.
- Criar um sistema de combate legível, com ataques sempre anunciados.
- Implementar checkpoints por fase com regras verificáveis.
- Organizar o código em camadas independentes e testáveis.
- Validar o jogo com testes automatizados e com uma build executável fora do Unity.

## 4. Inspirações

- **Lenda do Rei Arthur:** a espada na pedra e a ideia de que o portador precisa ser digno, não apenas herdeiro.
- **Soul Knight:** masmorras curtas em sequência, combate em tempo real e tentativas rápidas.

## 5. História e universo

A narrativa completa está em [HISTORIA.md](HISTORIA.md). Em resumo, o Rei-Feiticeiro Vorg fundou o reino de Valdren e, antes de morrer, cravou sua espada em uma pedra, selando três portões para que ela só fosse retirada por alguém digno. Cada portão representa uma provação: coragem na Cripta de Musgo, perseverança na Forja Afundada e legitimidade no Santuário de Cinzas. O protagonista, descendente do rei, precisa vencer as três para concluir a jornada.

A história não é apresentada como texto ou cena dentro do jogo. O jogo transmite o contexto por meio do título, do slogan, dos nomes das fases, das frases nos cartões de transição e da mensagem de vitória.

## 6. Gênero e estilo artístico

O jogo usa estética low poly com câmera isométrica ortográfica. As três fases têm arquiteturas próprias:

| Fase | Arquitetura | Iluminação |
|---|---|---|
| Cripta de Musgo | Catacumba de pedra com arcos, colunas redondas, musgo e vegetação | A mais clara, com tochas quentes |
| Forja Afundada | Estrutura industrial de ferro, vigas, grades e canais de lava | Escura, com calor localizado em laranja |
| Santuário de Cinzas | Catedral gótica com vitrais, rosáceas e colunas ornamentadas | A mais sombria, em tons violeta |

O dourado marca os elementos importantes, como recompensas, a espada e a interface.

## 7. Mecânicas de gameplay

O ciclo principal é: entrar na masmorra, explorar as salas, vencer os combates, coletar recompensas, derrotar o chefão e atravessar o portal para a fase seguinte.

- Ao entrar em uma sala de combate, as portas se fecham até todos os inimigos serem derrotados.
- Limpar uma sala de combate restaura 30 pontos de vida.
- Inimigos derrotados têm 35% de chance de deixar um orbe de cura de 15 pontos.
- A sala lateral oferece uma arma nova ou um elixir de cura.
- O inventário tem três espaços, um por arma.
- A campanha termina quando Vorg retira a espada na Câmara da Espada.

### Personagem

| Atributo | Valor |
|---|---|
| Vida | 100 |
| Velocidade | 6 m/s |
| Esquiva | 5 m em 0,25 s, com invulnerabilidade durante o movimento |
| Recarga da esquiva | 0,8 s |

## 8. Sistema de combate

O combate é em tempo real. O personagem sempre encara o cursor do mouse.

| Arma | Dano | Alcance | Característica |
|---|---:|---:|---|
| Espada | 20 | 2,2 m | Arco de 110°. O terceiro golpe seguido causa 30 |
| Machado | 45 | 2,6 m | Arco de 170°, lento e com empurrão forte |
| Cajado | 14 | 14 m | Projétil mágico à distância |

Cada ataque tem fases de preparação, janela ativa e recuperação, definidas em dados. Os golpes corpo a corpo verificam linha de visão, então não atravessam paredes.

### Inimigos

| Inimigo | Comportamento | Vida | Dano |
|---|---|---:|---:|
| Rastejante | Persegue e morde de perto | 40 | 10 |
| Arqueiro Ossudo | Mantém distância e dispara flechas | 30 | 8 |
| Bruto | Lento, investe em linha reta | 120 | 20 |

Todo ataque é anunciado por um aviso visual antes de causar dano. No máximo três inimigos atacam à distância ao mesmo tempo em uma sala.

### Chefões

| Chefão | Vida | Padrões de ataque |
|---|---:|---|
| Guardião da Cripta | 420 | Impacto circular e investida |
| Carrasco da Forja | 640 | Corte em cone, leque de brasas e erupções no chão |
| Sentinela de Vorg | 900 | Anel de projéteis, explosões marcadas e teleporte com impacto |

Os chefões desenham no chão a área real de cada ataque antes de executá-lo. Abaixo de 50% de vida, a preparação e a recuperação dos ataques ficam mais rápidas.

## 9. Sistema de geração procedural

Cada masmorra é gerada a partir de uma semente, em duas etapas separadas:

1. **Layout (C# puro):** uma caminhada sem repetição em uma grade define o caminho principal da entrada até a saída, com retrocesso quando necessário. Em seguida, são definidos os tipos de sala, a sala lateral e o orçamento de inimigos, que cresce ao longo do caminho.
2. **Validação:** o layout é verificado quanto à conectividade, quantidade e tipos de salas, sobreposição e recompensas. São feitas até 20 tentativas. Se todas falharem, um layout de segurança é usado.
3. **Construção:** as salas são instanciadas a partir dos prefabs da fase, e a malha de navegação dos inimigos é gerada em tempo de execução a partir dos colisores.

| Fase | Salas no caminho principal | Sala lateral | Inimigos por combate |
|---|---:|---:|---|
| Cripta de Musgo | 6 | 1 | 2 a 4 |
| Forja Afundada | 7 | 1 | 3 a 5 |
| Santuário de Cinzas | 7 | 1 | 3 a 5 |

As salas medem 18 × 18 m e ficam em uma grade com passo de 24 m. Uma assinatura do layout garante que cada nova tentativa gere um caminho diferente do anterior.

## 10. Sistema de checkpoints

1. Morrer em uma fase reinicia somente essa fase.
2. A fase reiniciada gera um novo layout.
3. O jogador mantém as armas que possuía ao entrar na fase.
4. Armas coletadas na tentativa fracassada são perdidas.
5. A vida é restaurada ao reiniciar.
6. O checkpoint só é gravado depois que a próxima fase está pronta, em um protocolo de duas etapas.
7. O progresso vale durante a sessão. Ao fechar o jogo, a campanha recomeça.

## 11. Fases

- **Cripta de Musgo:** inimigos Rastejantes e Arqueiros Ossudos. A sala lateral oferece o Machado. Termina com o Guardião da Cripta e um portal.
- **Forja Afundada:** os Brutos se juntam aos inimigos anteriores, com mais vida e dano. A sala lateral oferece o Cajado. Termina com o Carrasco da Forja e um portal.
- **Santuário de Cinzas:** encontros maiores com os três tipos de inimigos. Termina com a Sentinela de Vorg e a Câmara da Espada.

## 12. Personagens

| Personagem | Descrição |
|---|---|
| Vorg | Cavaleiro de armadura, elmo com plumagem, escudo e capa vermelha. Herdeiro do antigo rei |
| Rastejante | Criatura de carapaça com várias patas |
| Arqueiro Ossudo | Esqueleto arqueiro da antiga guarda |
| Bruto | Criatura grande e pesada da forja |
| Guardião da Cripta | Golem de pedra e musgo com núcleo de runas e escudo |
| Carrasco da Forja | Cavaleiro de armadura escurecida, capa ardente e espada longa |
| Sentinela de Vorg | Último cavaleiro da guarda real, com energia violeta |

## 13. Controles

| Ação | Entrada |
|---|---|
| Mover | WASD |
| Mirar | Mouse |
| Atacar | Clique esquerdo |
| Esquivar | Espaço |
| Interagir | E |
| Trocar arma | 1, 2, 3 ou roda do mouse |
| Pausar | Esc |

## 14. Tecnologias utilizadas

| Tecnologia | Versão | Uso |
|---|---|---|
| Unity | 6000.6.10f1 (Unity 6.6) | Motor do jogo |
| Universal Render Pipeline | 17.6.0 | Renderização |
| C# | Back-end Mono | Linguagem de programação |
| Input System | 1.20.1 | Entrada de mouse e teclado |
| Cinemachine | 6.6.0 | Câmera isométrica |
| UI Toolkit | Integrado ao Unity | Menus e HUD |
| Unity Test Framework | 1.8.0 | Testes automatizados |
| Git e Git LFS | Git LFS 3.8.1 | Controle de versão |

### Declaração de ferramentas de apoio

O desenvolvimento contou com o assistente de programação Claude Code (Anthropic), usado sob direção e aprovação dos alunos para implementação de código, geração de conteúdo procedural, automação de testes e builds e redação de documentação. As decisões de design, a aprovação de cada etapa e os testes manuais de jogo foram feitos pelos alunos. O pacote `com.unity.pipeline` foi usado para automatizar o Editor durante o desenvolvimento e não faz parte da build distribuída.

## 15. Arquitetura técnica

O código é dividido em Assembly Definitions com dependência em uma só direção:

```text
VorgGates.App  (composição e fluxo do jogo)
   ├─► VorgGates.Presentation  (HUD, menus, efeitos, áudio, animação visual)
   └─► VorgGates.Gameplay      (jogador, combate, inimigos, salas, masmorra)
            └─► VorgGates.Core (regras em C# puro, sem dependência do Unity)
```

| Módulo | Arquivos | Linhas | Responsabilidade |
|---|---:|---:|---|
| Core | 11 | 750 | Sessão, checkpoints, inventário, regras de combate, gerador e validador de layout |
| Gameplay | 33 | 2.080 | Jogador, armas, inimigos, chefões, salas, portas e construção da masmorra |
| Presentation | 19 | 1.407 | Interface, áudio, efeitos, câmera e animações visuais |
| App | 1 | 235 | Ponto de composição e fluxo de estados |
| Editor | 21 | 4.155 | Construção do conteúdo, materiais, áudio sintetizado e builds |
| Testes | 11 | 1.794 | Testes automatizados |

Decisões principais:

- **Regras isoladas:** a lógica de sessão, checkpoint e geração não depende do Unity e é testada em milissegundos.
- **Máquina de estados global:** `GameFlow` controla Menu, Carregando, Jogando, Pausado, Morrendo, Transição e Vitória, e rejeita transições inválidas.
- **Inimigos:** uma máquina de estados compartilhada, configurada por dados, com estratégias de ataque por tipo e padrões específicos para os chefões.
- **Dados em ScriptableObjects:** armas, inimigos e fases são configurados sem alterar código.
- **Separação visual:** os modelos e animações são filhos visuais das entidades. Colisores, navegação e regras ficam nas entidades, o que permitiu trocar a arte sem alterar a gameplay.
- **Cena única:** a masmorra é construída e destruída sob um objeto raiz, sem estado estático mutável.

## 16. Requisitos de sistema

### Configuração testada

| Item | Configuração |
|---|---|
| Sistema operacional | Windows 11 Home, 64 bits |
| Processador | AMD Ryzen 7 7700X |
| Memória | 16 GB |
| Placa de vídeo | NVIDIA GeForce RTX 5060 Ti, 8 GB, Direct3D 12 |
| Resoluções testadas | 1920 × 1080 e 1280 × 720 |

Nessa configuração, o executável final manteve média acima de 1.300 quadros por segundo em 1080p durante as lutas contra os chefões, medida pelo teste automatizado da build.

### Requisitos mínimos estimados (não testados)

| Item | Estimativa |
|---|---|
| Sistema operacional | Windows 10 de 64 bits |
| Processador | Quatro núcleos, a partir de 2015 |
| Memória | 4 GB |
| Placa de vídeo | Compatível com Direct3D 11 |
| Armazenamento | 200 MB livres |

Os requisitos mínimos são uma estimativa baseada no desempenho medido e no tamanho da build. O jogo não foi testado em computadores de menor desempenho.

## 17. Testes e validações

| Validação | Resultado na versão final |
|---|---|
| Compilação | Sem erros |
| Testes EditMode (regras, gerador, combate e campanha) | 45 de 45 aprovados |
| Testes PlayMode (fluxo, combate, campanha e interface) | 36 aprovados. O teste da campanha completa por robô falhou uma vez por tempo limite e passou nas duas repetições seguintes |
| Campanha completa no executável Windows, por robô | 22 de 22 verificações aprovadas, sem erros no log |
| Visibilidade da câmera em 24 layouts gerados | O personagem nunca fica escondido |
| Comparação das regras de gameplay | Valores, colisores e navegação idênticos à versão aprovada |
| Execução a partir do ZIP, fora do Unity | Aprovada |
| Teste manual completo | Campanha concluída por um dos desenvolvedores |

Os testes automatizados verificam, entre outros pontos, as regras de checkpoint, a conectividade de milhares de layouts gerados, a navegação entre todas as salas, os padrões dos chefões e a campanha completa do início à vitória.

## 18. Créditos e licenças

O jogo usa assets gratuitos com licenças compatíveis, detalhados em [ASSET_CREDITS.md](ASSET_CREDITS.md):

- **Undead Knight [Low-Poly]**, de SchrisArt, licenciado sob CC BY 4.0, usado no Carrasco da Forja com atribuição obrigatória.
- Kits modulares de SigilsVault e Seahill Studio, Fullplate Armor Knight de Luana Coppio e assets da Kenney, todos CC0.
- Fonte Cinzel, sob SIL Open Font License 1.1.

Trilhas, efeitos sonoros, interface, o personagem principal, os inimigos comuns e o Guardião da Cripta foram criados para o projeto.

## 19. Considerações finais

Vorg'Gates foi concluído como um jogo completo, com três fases, três chefões, geração procedural, checkpoints e uma build distribuível para Windows. A separação entre regras, entidades e camada visual permitiu evoluir a arte em várias etapas sem alterar a gameplay aprovada, o que foi confirmado por testes automatizados a cada mudança.

Como limitações, o progresso não é salvo em disco, o jogo não foi testado em computadores de menor desempenho e não há suporte a controle. Esses pontos ficam como possibilidades para trabalhos futuros.

## Documentos relacionados

- [HISTORIA.md](HISTORIA.md): história e universo.
- [TRAILER.md](TRAILER.md): roteiro do trailer.
- [PUBLICACAO.md](PUBLICACAO.md): textos para a página do jogo.
