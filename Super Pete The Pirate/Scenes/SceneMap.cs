﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Maps.Tiled;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.ViewportAdapters;
using Super_Pete_The_Pirate.Characters;
using Super_Pete_The_Pirate.Managers;
using Super_Pete_The_Pirate.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Super_Pete_The_Pirate.Scenes
{
    class SceneMap : SceneBase
    {
        //--------------------------------------------------
        // Player

        private Player _player;
        public Player Player { get { return _player; } }

        //--------------------------------------------------
        // Enemies

        private List<Enemy> _enemiesToRemove;
        private List<Enemy> _enemies;
        public List<Enemy> Enemies { get { return _enemies; } }

        //--------------------------------------------------
        // Projectiles

        private Dictionary<string, Texture2D> _projectilesTextures;
        private Texture2D _projectilesColliderTexture;
        private List<GameProjectile> _projectilesToRemove;
        private List<GameProjectile> _projectiles;
        private SoundEffect _shotSe;

        //--------------------------------------------------
        // Shops

        private List<GameShop> _shops;

        //--------------------------------------------------
        // Checkpoints

        private List<GameCheckpoint> _checkpoints;
        private CheckpointData _lastCheckpoint;
        private struct CheckpointData
        {
            public bool Activated;
            public GameCheckpoint Checkpoint;
            public bool UsePosition;
            public Vector2 Position;
            public List<Enemy> MapEnemies;
            public List<GameCoin> MapCoins;
            public int Ammo;
            public int Coins;
            public int CoinsCollected;
        }

        //--------------------------------------------------
        // Coins

        private List<GameCoin> _coinsToRemove;
        private List<GameCoin> _coins;
        private SoundEffect _coinsSe;

        //--------------------------------------------------
        // Deflect SE

        private SoundEffect _deflectSe;

        //--------------------------------------------------
        // Camera stuff

        private Camera2D _camera;
        private const float CameraSmooth = 0.1f;
        private const int PlayerCameraOffsetX = 40;

        //--------------------------------------------------
        // Player Hud

        private GameHud _gameHud;

        //--------------------------------------------------
        // Random

        private Random _rand;

        //--------------------------------------------------
        // Ambience

        private SoundEffectInstance _ambienceSe;

        //--------------------------------------------------
        // Stage Completed Helper

        private bool _finishStageCalled;
        private bool _stageCompleted;
        private SceneMapSCHelper _stageCompletedHelper;

        //--------------------------------------------------
        // Pause Helper

        private SceneMapPauseHelper _pauseHelper;

        //--------------------------------------------------
        // Background Helper

        private SceneMapBackgroundHelper _backgroundHelper;

        //--------------------------------------------------
        // Track variables

        private int _coinsCollected;
        private int _maxCoins;
        private int _heartsLost;
        private int _enemiesDefeated;
        private int _maxEnemies;
        private TimeSpan _time;

        //----------------------//------------------------//

        public override void LoadContent()
        {
            base.LoadContent();

            var viewportSize = SceneManager.Instance.VirtualSize;
            _camera = new Camera2D(SceneManager.Instance.ViewportAdapter);

            // Player init
            _player = new Player(ImageManager.loadCharacter("Player"));

            // Enemies init
            _enemies = new List<Enemy>();
            _enemiesToRemove = new List<Enemy>();

            // Projectiles init
            _projectilesTextures = new Dictionary<string, Texture2D>()
            {
                {"common", ImageManager.loadProjectile("Common")},
                {"cannonball", ImageManager.loadProjectile("Cannonball")}
            };
            _projectilesToRemove = new List<GameProjectile>();
            _projectiles = new List<GameProjectile>();
            _shotSe = SoundManager.LoadSe("Shot");
            _projectilesColliderTexture = new Texture2D(SceneManager.Instance.GraphicsDevice, 1, 1);
            _projectilesColliderTexture.SetData(new Color[]{ Color.Orange });

            // Shops init
            _shops = new List<GameShop>();

            // Checkpoints init
            _checkpoints = new List<GameCheckpoint>();

            // Coins init
            _coinsToRemove = new List<GameCoin>();
            _coins = new List<GameCoin>();
            _coinsSe = SoundManager.LoadSe("Coins");

            // Random init
            _rand = new Random();

            // Stage Complete Helper init
            _stageCompletedHelper = new SceneMapSCHelper();

            // Pause helper init
            _pauseHelper = new SceneMapPauseHelper();

            // Ambience SE init
            var ambienceSe = SoundManager.LoadSe("Ambience");
            if (_ambienceSe != null)
            {
                _ambienceSe = ambienceSe.CreateInstance();
                _ambienceSe.IsLooped = true;
            }

            // SEs init
            _deflectSe = SoundManager.LoadSe("Deflect");

            // Load the map
            LoadMap(SceneManager.Instance.MapToLoad);

            // Create the HUD
            CreateHud();

            // Background init
            _backgroundHelper = new SceneMapBackgroundHelper();

            // Ambience SE
            _ambienceSe.PlaySafe();

            // Save the initial data
            PlayerManager.Instance.StoreData();
        }

        public override void UnloadContent()
        {
            foreach (var enemy in _enemies)
            {
                enemy.DisposeEnemyTextures();
                enemy.Dispose();
            }
            _player.DisposeTextures();
            _player.Dispose();
            _pauseHelper.Dispose();
            _stageCompletedHelper.Dispose();
            _projectilesColliderTexture.Dispose();
            GameMap.Instance.UnloadMap();
            base.UnloadContent();
        }

        private void InitializeTrackVariables()
        {
            _time = new TimeSpan();
            _maxCoins = _coins.Count;
            _enemies.ForEach(enemy => _maxCoins += enemy.Coins);
            _maxEnemies = _enemies.Count;
        }

        private void CreateHud()
        {
            _gameHud = new GameHud(ImageManager.loadSystem("IconsSpritesheet"));
            _gameHud.SetPosition(new Vector2(5, 5),
                new Vector2(SceneManager.Instance.VirtualSize.X - 45, 5));
        }

        private void LoadMap(int mapId)
        {
            GameMap.Instance.LoadMap(Content, mapId);

            var bgm = GameMap.Instance.GetSongName() ?? "AngevinB";
            SoundManager.StartBgm(bgm);

            SpawnShops();
            SpawnEnemies();
            SpawnCheckpoints();
            SpawnCoins();
            SpawnPlayer();

            InitializeTrackVariables();
        }

        private void SpawnCoins()
        {
            var coinsGroup = GameMap.Instance.GetObjectGroup("Coins");
            if (coinsGroup == null) return;

            foreach (var coinObj in coinsGroup.Objects)
            {
                CreateCoin(Convert.ToInt32(coinObj.X), Convert.ToInt32(coinObj.Y - 32), Vector2.Zero, false);
            }
        }

        public GameCoin CreateCoin(int x, int y, Vector2 velocity, bool applyPhyics)
        {
            var coinTexture = ImageManager.loadMisc("Coin");
            var coinFrames = new Rectangle[]
            {
                new Rectangle(0, 0, 32, 32),
                new Rectangle(32, 0, 32, 32),
                new Rectangle(64, 0, 32, 32),
                new Rectangle(96, 0, 32, 32)
            };
            var coinBoudingBox = new Rectangle(8, 8, 16, 16);
            var coin = new GameCoin(coinTexture, coinFrames, 120, x, y, velocity, applyPhyics);
            coin.CoinSprite.SetBoundingBox(coinBoudingBox);
            _coins.Add(coin);
            return coin;
        }

        public void SpawnCheckpoints()
        {
            var checkpointGroup = GameMap.Instance.GetObjectGroup("Checkpoints");
            if (checkpointGroup == null) return;

            foreach (var checkpointObj in checkpointGroup.Objects)
            {
                var endFlag = checkpointObj.Properties.ContainsKey("EndFlag") && checkpointObj.Properties["EndFlag"] == "true";
                CreateCheckpoints(Convert.ToInt32(checkpointObj.X), Convert.ToInt32(checkpointObj.Y - 96), endFlag);
            }
        }

        private GameCheckpoint CreateCheckpoints(int x, int y, bool endFlag)
        {
            var checkpointTexture = ImageManager.loadMisc("CheckPointSpritesheet");
            var yInc = endFlag ? 192 : 0;
            var checkpointFrames = new Rectangle[]
            {
                new Rectangle(0, yInc, 64, 96),
                new Rectangle(64, yInc, 64, 96),
                new Rectangle(128, yInc, 64, 96)
            };
            var checkpointBoundingBox = new Rectangle(0, 24, 37, 72);
            var checkpoint = new GameCheckpoint(checkpointTexture, checkpointFrames, 130, x, y, endFlag);
            checkpoint.SetBoundingBox(checkpointBoundingBox);
            _checkpoints.Add(checkpoint);
            return checkpoint;
        }

        private void SpawnPlayer()
        {
            var spawnPoint = new Vector2(GameMap.Instance.GetPlayerSpawn().X, GameMap.Instance.GetPlayerSpawn().Y - _player.CharacterSprite.GetColliderHeight());
            _player.Position = new Vector2(spawnPoint.X, spawnPoint.Y);
            CreatePositionCheckpoint(spawnPoint);
        }

        private void SpawnEnemies()
        {
            _enemies.Clear();
            var enemiesGroup = GameMap.Instance.GetObjectGroup("Enemies");
            if (enemiesGroup == null) return;
            var tileSize = GameMap.Instance.TileSize;
            foreach (var enemieObj in enemiesGroup.Objects)
            {
                CreateEnemy(enemieObj, Convert.ToInt32(enemieObj.X), Convert.ToInt32(enemieObj.Y));
            }
        }

        public void CreateEnemy(TiledObject enemyObj, int x, int y)
        {
            var enemyName = "";
            if (enemyObj.Properties.ContainsKey("type"))
            {
                enemyName = enemyObj.Properties["type"];
            }
            else if (enemyObj.Properties.ContainsKey("Type"))
            {
                enemyName = enemyObj.Properties["Type"];
            }
            else
            {
                return;
            }
            var texture = ImageManager.loadCharacter(enemyName);
            var newEnemy = (Enemy)Activator.CreateInstance(Type.GetType("Super_Pete_The_Pirate.Characters." + enemyName), texture);
            newEnemy.Position = new Vector2(x, y - 32);
            if (enemyObj.Properties.ContainsKey("FlipHorizontally"))
                newEnemy.CharacterSprite.Effect = enemyObj.Properties["FlipHorizontally"] == "true" ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (enemyName == "Parrot")
            {
                var width = enemyObj.Properties.ContainsKey("FlyWidth") ? int.Parse(enemyObj.Properties["FlyWidth"]) : 224;
                ((Parrot)newEnemy).SetFlyWidth(width);
                var rangeDec = newEnemy.CharacterSprite.Effect.HasFlag(SpriteEffects.FlipHorizontally) ? width : 0;
                ((Parrot)newEnemy).SetFlyRange((int)newEnemy.Position.X - rangeDec, (int)newEnemy.Position.Y);
            }

            if (enemyName == "Mole") ((Mole)newEnemy).SetHolePoint((int)newEnemy.Position.X, y);

            _enemies.Add(newEnemy);
        }

        private void SpawnShops()
        {
            var shopsGroup = GameMap.Instance.GetObjectGroup("Shops");
            if (shopsGroup == null) return;
            foreach (var shopObj in shopsGroup.Objects)
            {
                CreateShop(shopObj, Convert.ToInt32(shopObj.X), Convert.ToInt32(shopObj.Y));
            }
        }

        private void CreateShop(TiledObject shopObj, int x, int y)
        {
            var shopType = shopObj.Properties["Type"];
            if (shopType == null) return;
            var shopTexture = ImageManager.loadMisc("Panel" + shopType);
            var type = GameShopType.None;
            switch (shopType)
            {
                case "Ammo":
                    type = GameShopType.Ammo;
                    break;
                case "Hearts":
                    type = GameShopType.Hearts;
                    break;
                case "Lives":
                    type = GameShopType.Lives;
                    break;
            }
            var arrowsTexture = ImageManager.loadSystem("ShopUpButton");
            var shop = new GameShop(type, shopTexture, arrowsTexture, new Vector2(x, y));
            _shops.Add(shop);
        }

        public void CreateProjectile(string name, Vector2 position, int dx, int dy, int damage, ProjectileSubject subject)
        {
            if (name == "common")
                _shotSe.PlaySafe();
            _projectiles.Add(new GameProjectile(_projectilesTextures[name], position, dx, dy, damage, subject));
        }

        private void CreatePositionCheckpoint(Vector2 position)
        {
            var checkpointData = new CheckpointData()
            {
                Activated = true,
                UsePosition = true,
                Position = position,
                MapEnemies = new List<Enemy>(_enemies.Select(enemy => enemy.Clone<Enemy>())),
                MapCoins = new List<GameCoin>(_coins.Select(coin => coin.Clone())),
                Ammo = PlayerManager.Instance.Ammo,
                Coins = PlayerManager.Instance.Coins,
                CoinsCollected = _coinsCollected
            };
            _lastCheckpoint = checkpointData;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Helpers
            if (_stageCompleted)
                _stageCompletedHelper.Update(gameTime);
            _backgroundHelper.Update(_camera);
            _pauseHelper.Update(gameTime);

            if (_pauseHelper.Paused) return;

            _time += gameTime.ElapsedGameTime.Duration();
            DebugValues["Timer"] = _time.ToString();

            var bossCollapsing = GameMap.Instance.CurrentMapId == 5 && _enemies.Count > 0 && ((Boss)_enemies[0]).Collapsing;
            _player.Update(gameTime, _stageCompleted || bossCollapsing);

            if (_player.RequestRespawn)
                HandlePlayerRespawn();

            foreach (var projectile in _projectiles)
            {
                projectile.Update(gameTime);
                if (projectile.Subject == ProjectileSubject.FromEnemy && projectile.BoundingBox.Intersects(_player.BoundingRectangle))
                {
                    var lastHearts = _player.HP;
                    _player.ReceiveAttack(projectile.Damage, projectile.LastPosition);
                    if (lastHearts - _player.HP > 0)
                        _heartsLost += lastHearts - _player.HP;
                }

                if (projectile.RequestErase)
                    _projectilesToRemove.Add(projectile);
            }

            foreach (var enemy in _enemies)
            {
                enemy.Update(gameTime);

                if (enemy.HasViewRange && enemy.ViewRangeCooldown <= 0f &&
                    _camera.Contains(enemy.BoundingRectangle) != ContainmentType.Disjoint &&
                    enemy.ViewRange.Intersects(_player.BoundingRectangle))
                {
                    enemy.PlayerOnSight(_player.Position);
                }

                if (!enemy.Dying && enemy.ContactDamageEnabled && enemy.BoundingRectangle.Intersects(_player.BoundingRectangle))
                {
                    var lastHearts = _player.HP;
                    _player.ReceiveAttackWithRect(1, enemy.BoundingRectangle);
                    if (lastHearts - _player.HP > 0)
                        _heartsLost += lastHearts - _player.HP;
                }

                foreach (var projectile in _projectiles)
                {
                    if (projectile.Subject == ProjectileSubject.FromPlayer)
                    {
                        if (!enemy.Dying && !enemy.IsImunity && enemy.CanReceiveAttacks && projectile.BoundingBox.Intersects(enemy.BoundingRectangle))
                        {
                            if (enemy.EnemyType == EnemyType.TurtleWheel && enemy.InWheelMode)
                            {
                                projectile.Acceleration = new Vector2(projectile.Acceleration.X * -1.7f, _rand.Next(-4, 5));
                                _deflectSe.PlaySafe();
                                CreateSparkParticle(projectile.Position);
                                projectile.Subject = ProjectileSubject.FromEnemy;
                            }
                            else
                            {
                                enemy.ReceiveAttack(projectile.Damage, projectile.LastPosition);
                                projectile.Destroy();
                            }
                        }
                    }
                    else if (projectile.BoundingBox.Intersects(_player.BoundingRectangle))
                    {
                        var lastHearts = _player.HP;
                        _player.ReceiveAttack(projectile.Damage, projectile.LastPosition);
                        if (lastHearts - _player.HP > 0)
                            _heartsLost += lastHearts - _player.HP;
                        projectile.Destroy();
                    }

                    if (projectile.RequestErase)
                        _projectilesToRemove.Add(projectile);
                }

                if (enemy is Boss)
                {
                    var boss = (Boss)enemy;
                    if (boss.RequestingHatDrop)
                    {
                        _player.PerformHatDrop();
                    }
                    if (boss.Collapsing)
                    {
                        SoundManager.SetBgmVolume(0.3f);
                        if (_ambienceSe != null)
                        {
                            _ambienceSe.Volume = 0.3f;
                        }
                    }
                }
                if (enemy.RequestErase)
                {
                    _enemiesToRemove.Add(enemy);
                    _enemiesDefeated++;
                }
            }

            foreach (var spike in GameMap.Instance.Spikes)
            {
                if (_player.BoundingRectangle.Intersects(spike) && !_player.TouchedSpikes)
                {
                    _heartsLost += _player.HP;
                    _player.CharacterSprite.RemoveImmunity();
                    _player.ReceiveAttackWithRect(999, spike);
                    _player.TouchedSpikes = true;
                }
            }

            foreach (var coin in _coins)
            {
                coin.Update(gameTime);
                var sprite = coin.CoinSprite;
                if (sprite.TextureRegion.Name.IndexOf("CoinSparkle") > 0 && sprite.Looped)
                {
                    _coinsToRemove.Add(coin);
                }
                else if (sprite.TextureRegion.Name.IndexOf("CoinSparkle") < 0 && _player.BoundingRectangle.Intersects(sprite.BoundingBox))
                {
                    _coinsCollected += 1;
                    PlayerManager.Instance.AddCoins(1);
                    sprite.SetTexture(ImageManager.loadMisc("CoinSparkle"), false);
                    sprite.SetDelay(80);
                    _coinsSe.PlaySafe();
                }
            }

            foreach (var shop in _shops)
            {
                if (shop.IsActive && !_player.BoundingRectangle.Intersects(shop.BoundingRectangle))
                {
                    shop.SetActive(false);
                }
                else if (!shop.IsActive && _player.BoundingRectangle.Intersects(shop.BoundingRectangle))
                {
                    shop.SetActive(true);
                }

                if (shop.IsActive && ((_player.IsAttacking && !shop.IsDenied()) || shop.NeedDeny()))
                {
                    shop.SetArrowDenyState();
                }
                else if (shop.IsDenied() && !_player.IsAttacking)
                {
                    shop.SetArrowNormalState();
                }
                shop.Update(gameTime);
            }

            foreach (var checkpoint in _checkpoints)
            {
                if (!checkpoint.IsChecked && _player.BoundingRectangle.Intersects(checkpoint.BoundingBox))
                {
                    if (checkpoint.IsEndFlag)
                    {
                        FinishStage(false);
                    }
                    else
                    {
                        checkpoint.OnPlayerCheck();
                        var checkpointData = new CheckpointData()
                        {
                            Activated = true,
                            Checkpoint = checkpoint,
                            MapEnemies = new List<Enemy>(_enemies.Select(enemy => enemy.Clone<Enemy>())),
                            MapCoins = new List<GameCoin>(_coins.Select(coin => coin.Clone())),
                            Ammo = PlayerManager.Instance.Ammo,
                            Coins = PlayerManager.Instance.Coins,
                            CoinsCollected = _coinsCollected
                        };
                        _lastCheckpoint = checkpointData;
                    }
                }
                checkpoint.Update(gameTime);
            }

            foreach (var enemy in _enemiesToRemove)
            {
                enemy.DisposeEnemyTextures();
                enemy.Dispose();
                _enemies.Remove(enemy);
            }

            foreach (var projectile in _projectilesToRemove)
            {
                _projectiles.Remove(projectile);
            }

            foreach (var coin in _coinsToRemove)
            {
                _coins.Remove(coin);
            }

            _enemiesToRemove.Clear();
            _projectilesToRemove.Clear();
            _coinsToRemove.Clear();

            UpdateCamera();
        }

        private void UpdateCamera()
        {
            var size = SceneManager.Instance.WindowSize;
            var viewport = SceneManager.Instance.ViewportAdapter;
            var newPosition = _player.Position - new Vector2(viewport.VirtualWidth / 2f, viewport.VirtualHeight / 2f);
            var playerOffsetX = PlayerCameraOffsetX + _player.CharacterSprite.GetColliderWidth() / 2;
            var playerOffsetY = _player.CharacterSprite.GetFrameHeight() / 2;
            var x = MathHelper.Lerp(_camera.Position.X, newPosition.X + playerOffsetX, CameraSmooth);
            x = MathHelper.Clamp(x, 0.0f, GameMap.Instance.MapWidth - viewport.VirtualWidth);
            var y = MathHelper.Lerp(_camera.Position.Y, newPosition.Y + playerOffsetY, CameraSmooth);
            y = MathHelper.Clamp(y, 0.0f, GameMap.Instance.MapHeight - viewport.VirtualHeight);
            _camera.Position = new Vector2((int)x, (int)y);
        }

        private void HandlePlayerRespawn()
        {
            if (_finishStageCalled) return;
            if (PlayerManager.Instance.Lives > 1)
            {
                PlayerManager.Instance.HandleRespawn();
                _player.DisposeTextures();
                _player.Dispose();
                _player = new Player(ImageManager.loadCharacter("Player"));
                if (_lastCheckpoint.UsePosition)
                {
                    var position = _lastCheckpoint.Position;
                    _player.Position = new Vector2(position.X, position.Y);
                }
                else
                {
                    var position = _lastCheckpoint.Checkpoint.Position;
                    _player.Position = new Vector2(position.X, position.Y + _player.CharacterSprite.GetColliderHeight());
                }
                foreach (var enemy in _enemies)
                {
                    enemy.DisposeEnemyTextures();
                    enemy.Dispose();
                }
                _enemies.Clear();
                _enemies = new List<Enemy>(_lastCheckpoint.MapEnemies.Select(x => x.Clone<Enemy>()));
                _coins = new List<GameCoin>(_lastCheckpoint.MapCoins.Select(x => x.Clone()));
                PlayerManager.Instance.SetAmmo(_lastCheckpoint.Ammo);
                PlayerManager.Instance.SetCoins(_lastCheckpoint.Coins);
                _coinsCollected = _lastCheckpoint.CoinsCollected;
                _enemiesDefeated = _maxEnemies - _lastCheckpoint.MapEnemies.Count;
            }
            else
            {
                PlayerManager.Instance.AddLives(-1);
                FinishStage(true);
                _finishStageCalled = true;
            }
        }

        private void CreateParticle()
        {
            var texture = ImageManager.loadParticle("WhitePoint");
            for (var i = 0; i < 20; i++)
            {
                var position = new Vector2(100, 100);
                var velocity = new Vector2(_rand.NextFloat(-100f, 100f), _rand.NextFloat(-500f, -300f));
                var color = ColorUtil.HSVToColor(MathHelper.ToRadians(_rand.NextFloat(0, 359)), 0.6f, 1f);
                var scale = _rand.Next(0, 2) == 0 ? new Vector2(2, 2) : new Vector2(3, 3);

                var state = new ParticleState()
                {
                    Velocity = velocity,
                    Type = ParticleType.Confetti,
                    Gravity = 1.8f,
                    UseCustomVelocity = true,
                    VelocityMultiplier = 0.95f
                };

                SceneManager.Instance.ParticleManager.CreateParticle(texture, position, color, 1000f, scale, state);
            }
        }

        public void CreateSparkParticle(Vector2 position, int number = 4)
        {
            var texture = ImageManager.loadParticle("Spark");
            for (var i = 0; i < number; i++)
            {
                var velocity = new Vector2(_rand.NextFloat(-100f, 100f), _rand.NextFloat(-100f, 100f));
                var scale = new Vector2(_rand.Next(7, 12), _rand.Next(1, 2));
                var color = ColorUtil.HSVToColor(MathHelper.ToRadians(51f), 0.5f, 0.91f);

                var state = new ParticleState()
                {
                    Velocity = velocity,
                    Type = ParticleType.Spark,
                    UseCustomVelocity = true,
                    VelocityMultiplier = 1f,
                    Width = (int)scale.X,
                    H = 57.0f
                };

                SceneManager.Instance.ParticleManager.CreateParticle(texture, position, color, 200f, scale, state);
            }
        }

        private void FinishStage(bool failed)
        {
            if (_stageCompleted) return;
            _stageCompleted = true;
            var data = new SceneMapSCHelper.StageCompletedData
            {
                CoinsCollected = _coinsCollected,
                MaxCoins = _maxCoins,
                HeartsLost = _heartsLost,
                EnemiesDefeated = _enemiesDefeated,
                MaxEnemies = _maxEnemies,
                Time = _time,
                MaxTime = GameMap.Instance.GetCompletionTime(),
                Failed = failed
            };
            _stageCompletedHelper.Initialize(data);
        }

        public int GetHeartsLost()
        {
            return _heartsLost;
        }

        private void CallSavesSceneToSave()
        {
            SceneManager.Instance.TypeOfSceneSaves = SceneManager.SceneSavesType.Save;
            SceneManager.Instance.ChangeScene("SceneSaves");
        }

        public override void Draw(SpriteBatch spriteBatch, ViewportAdapter viewportAdapter)
        {
            base.Draw(spriteBatch, viewportAdapter);
            var debugMode = SceneManager.Instance.DebugMode;

            _backgroundHelper.Draw(_camera, spriteBatch);

            // Draw the map
            GameMap.Instance.Draw(_camera, spriteBatch);

            spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);

            // Draw the shops
            for (var i = 0; i < _shops.Count; i++)
            {
                _shops[i].Draw(spriteBatch);
            }

            // Draw the checkpoints
            for (var i = 0; i < _checkpoints.Count; i++)
            {
                _checkpoints[i].Draw(spriteBatch);
            }

            // Draw the coins
            for (var i = 0; i < _coins.Count; i++)
            {
                _coins[i].CoinSprite.Draw(spriteBatch);
            }

            // Draw the player
            _player.DrawCharacter(spriteBatch);
            _player.DrawHat(spriteBatch);
            if (debugMode) _player.DrawColliderBox(spriteBatch);

            // Draw the enemies
            foreach (var enemy in _enemies)
            {
                enemy.DrawCharacter(spriteBatch);
                if (debugMode) enemy.DrawColliderBox(spriteBatch);
                if (enemy is Boss)
                {
                    ((Boss)enemy).DrawInnerSprites(spriteBatch);
                }
            }

            // Draw the projectiles
            foreach (var projectile in _projectiles)
            {
                spriteBatch.Draw(projectile.Sprite);
                if (debugMode) spriteBatch.Draw(_projectilesColliderTexture, projectile.BoundingBox, Color.White * 0.5f);
            }

            // Draw the particles
            SceneManager.Instance.ParticleManager.Draw(spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin(transformMatrix: SceneManager.Instance.ViewportAdapter.GetScaleMatrix(),
                samplerState: SamplerState.PointClamp);

            _player.DrawScreenFlash(spriteBatch);

            // Draw the Hud
            _gameHud.Draw(spriteBatch);

            // Draw the Boss Hud (if exists)
            ((Boss)_enemies.SingleOrDefault(enemy => enemy is Boss))?.Draw(spriteBatch);

            if (_stageCompleted)
                _stageCompletedHelper.Draw(spriteBatch);

            _pauseHelper.Draw(spriteBatch);

            spriteBatch.End();
        }
    }
}