# TP1-Tecnicas
Trabalho Prático 1, no âmbito da unidade curricular Técnicas de Desenvolvimento de VideoJogos - 2º semestre, 1º ano EDJD

Realizado por João Miguel Pereira - a24254

# Introdução
No âmbito da unidade curricular de Técnicas de Desenvolvimento de VideoJogos, foi proposto um trabalho que visava a análise de um jogo criado em monogame, do seu código e organização. Em resposta a esta proposta, o jogo que escolhi analisar foi o "Super Pete the Pirate", criado por Rafael Almeida. Neste jogo controlamos o gato Pete, um capitão pirata. Numa das suas viagens, Pete vê o seu navio ser saqueado por outros piratas quando atravessa um nevoeiro intenso. No meio do saque, os piratas levam algo muito importante para Pete, o seu chapéu. Após este golpe, Pete é expulso do seu navio e procura reaver o seu chapéu dos piratas que o roubaram.

# Estrutura do código
Este jogo contem várias classes, divididas em grupos:

-> Classes Principais;

-> Charecters;

-> Extensions;

-> Managers;

-> Objects;

-> Scenes;

-> Sprites;

# Classes

## Principais

As classes principais deste jogo são as classes GameMain e Program, responsáveis pelo funcionamento do jogo.

## Characters

As classes das characters são responsáveis pelos personagens do jogo, nos quais estão incluidos as seguintes:

-> Boss;

-> CharacterBase;

-> Enemy;

-> Mole;

-> Parrot;

-> Player;

-> SniperPig;

-> TurtleWheel;

## Extensions

As extensions são classes com poucas linhas de código, nas quais são gerados valores utilizados no jogo. Estas classes incluem as seguintes:

-> ColorUtil;

-> MathUtil;

-> RandomExtensions;

-> RectangleExtensions;

-> SpriteBatchExtensions;

-> Util;

-> Vector2Extensions;

## Managers

As classes managers, como o nome indica, são responsáveis pela gestãos de certos elementos do jogo. As classes managers são as seguintes:

-> IconsManager;

-> ImageManager;

-> InputManager;

-> ParticleManager;

-> ParticleState;

-> PlayerManager;

-> SavesManager;

-> SceneManager;

-> SettingsManager;

-> SoundManager;

## Objects

Nas classes objects encontramos classes responsáveis por objetos do jogo, como por exemplo objetos com os quais o jogador interage, fisicas de objetos, o hud do proprio jogo, checkpoints e o mapa. As classes enquadradas nesta categoria são as seguintes:

-> GameCannon;

-> GameCheckpoints;

-> GameCoin;

-> GameHud;

-> GameMap;

-> GameProjectile;

-> GameShop;

-> PhysicalObject;

## Scenes

As classes scenes são maioritariamente classes responsáveis por desenhar os elementos visuais do jogo no ecrã, como o mapa, o background, os itens. Esta categoria inclui as seguintes classes:

-> SceneBase;

-> SceneCredit;

-> SceneIntro;

-> SceneMap;

-> SceneMapBackgroundHelper;

-> SceneMapPauseHelper;

-> SceneMapSCHelper;

-> SceneSaves;

-> SceneStageSelect;

-> SceneTitle;

-> SceneTitleOptionsHelper;

## Sprites

Por fim, as classes incluidas na categoria sprites são responsáveis pelas sprites do jogador e dos varios elementos do jogo (outros personagens, itens, objetos) e pelo comporamento que estes têm quando interagem uns com os outros, como na sua sobreposição, por exemplo. Aqui encontramos as seguintes classes:

-> AnimatedSprite;

-> CharacterSprite;

-> FrameInfo;

-> FramesList;

-> SpriteCollider;

# Análise de algumas Classes

## Classe Player -  Characters

A classe player é responsável pelo personagem Pete, que é controlado pelo jogador. A classe contem os atributos de Pete, o seu construtor e os vários metodos, estando estes 3 organizados por esta ordem. Os atributos estão divididos em vários grupos, atráves de comentários, como por exemplo a sua vida pertencente ao grupo HP, o número de moedas que o jogador possui pertencente ao grupo Coins, etc. Possui um único construtor organizando os seus atributos e os respetivos valores também através das linhas de documentário. Por fim, a classe possui vários metodos, alguns utilizados para atualizar o estado do jogador ao longo do jogo, caso dos metodos Update e UpdateFrameList, e ainda alguns metodos relativos às proprias mecanicas do jogo, como ataques e movimentos. 

As restantes classes pertencentes à categoria Charecters estão organizadas da mesma maneira que a classe Player, possuindo atributos organizados por categorias, construtor e métodos utilizados na atualização ao longo do jogo e nas suas mecânicas.

## Classes Managers

As classes Managers, como o nome indica, são responsáveis pela gestão de vários elementos do jogo em si. É através destas várias classes que os elementos são organizados e distribuidos no carremento de um save, são guardados em saves quando é guardado o jogo e são também inicializados ou reiniciados na criação de um novo jogo. Com estas classes, obtem-se maior facilidade na organização dos metodos responsáveis por esta gestão dos vários elementos do jogo.

## Classes Objects

As classes Objects são responsáveis pela criação e atualização dos varios objetos utilizados durante o jogo. Estes elementos incluem itens do jogo, como moedas, objetos fisicos com os quais o jogador interage e projeteis. Nesta categoria de classes, estão ainda incluidos objetos com os quais o jogador não interage diretamente, mas fazem parte da jogabilidade, como checkpoints, mapa, hud. Estas classes estão organizadas da mesma maneira que as anteriores, ordenadas por atributos (estando estes divididos por categorias, com linhas de comentário a definir cada uma), contrutor(es) e métodos. 

# Análise Geral do Código

Todo o código do jogo foi organizado através de uma estrutura bem definida. As várias classes estão organizadas em grupos nas quais é possível assinalar pontos comuns entre as mesmas. Dentro de cada classe é evidente um método de organização comum, organizando os atributos, contrutores e métodos por esta mesma ordem. Os atríbutos referentes a cada classe estão ainda subdivididos em categorias, através de linhas de comentário. Cada classe possuí os seus métodos, alguns responsáveis pelas suas mecânicas do jogo, nomeadamente nas categoria de classes Charecters, e métodos responsáveis pela atualização dos vários elementos do jogo no decorrer do mesmo, sendo isto comum à maioria das classes e à maioria das categorias. 

# Conclusão 

Esta proposta de trabalho revelou-se uma ferramenta bastante útil no desenvolvimento das capacidades de programação de jogos, sublinhando a boa organização de todo o código como uma competência fundamental na área da programação de videojogos. Podemos facilmente concluir que a organização das várias classes em categorias, a divisão do código de cada classe em partes com objetivos diferentes e a documentação do mesmo, atráves de comentários, foram ferramentas fundamentais para a criação organizada deste jogo e as mesmas são fulcrais no desenvolvimento tanto de videojogos de grande dimensão como até nos mais básicos.

# Bibliografia

Link do repositório original do jogo:

https://github.com/rafaelalmeidatk/Super-Pete-The-Pirate
