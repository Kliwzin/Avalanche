# Avalanche

Tetris reconstruído do zero em Unity e depois quebrado de propósito:
blocos de aço que travam as linhas, runas que os desfazem, e uma
avalanche que desaba sobre o tabuleiro de tempos em tempos.

**[Jogar no navegador](https://kliwzin.itch.io/avalanche)** ·
**[Página do projeto](https://wilklacerda.com/jogos/avalanche.html)**

## Mecânicas

**Blocos de aço** aparecem no meio das peças e não podem ser
eliminados: uma linha não se completa enquanto houver aço nela.

**Blocos de runa** disparam um feixe pela coluna ao pousar e convertem
todo o aço que encontram. A chance de aparecerem cresce com a
quantidade de colunas presas — dificuldade e alívio sobem juntos.

**A avalanche** derruba uma fileira inteira sobre o tabuleiro a cada
tantas peças. O contador avisa quando; não avisa que, se o tabuleiro
estiver travado demais, ela vem inteira feita de runas.

**Guardar peça** (C) troca a peça atual pela reservada, uma vez por
peça. Guardar uma runa para o momento certo é a decisão mais
interessante do jogo.

## Controles

| Tecla | Ação |
|---|---|
| ← → | Mover |
| ↑ | Girar |
| ↓ | Descer rápido |
| Espaço | Queda instantânea |
| C | Guardar peça |
| Esc | Pausar |

## Abrir o projeto

Feito em Unity 6 (6000.6.0f1) com URP 2D. Abra a pasta pelo Unity Hub
e carregue `Assets/Scenes/SampleScene.unity`.

## Estrutura

Os scripts ficam em `Assets/Scripts`. Os principais:

- `GridManager` — a matriz do tabuleiro e a fonte única da verdade
  sobre onde está cada bloco. Limpeza de linhas, gravidade e avalanches.
- `Tetromino` — controle da peça ativa: movimento, rotação com wall
  kicks, DAS, hard drop e o travamento no grid.
- `Spawner` — sorteio com pesos, planejamento do metal por peça,
  previews e o sistema de guardar peça.
- `AvalancheManager` — ritmo das avalanches e escolha entre a normal
  e a de runas.

## Dívida técnica conhecida

O `Tetromino` conduz o fluxo do jogo: ao travar, é ele quem chama o
`GridManager`, o `AvalancheManager` e o `Spawner`. O certo seria um
controlador de partida próprio, com a peça apenas avisando que
terminou. Funciona e está testado, mas é a primeira coisa que eu
refatoraria.

## Créditos

Programação, mecânicas e integração: Wilk Lacerda.
Arte dos blocos, plano de fundo e animações: equipe do projeto.