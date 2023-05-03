﻿namespace Super_Pete_The_Pirate.Managers
{
    class PlayerManager
    {
        //--------------------------------------------------
        // Singleton variables

        private static PlayerManager _instance = null;
        private static readonly object _padlock = new object();

        public static PlayerManager Instance
        {
            get {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new PlayerManager();
                    return _instance;
                }
            }
        }

        //--------------------------------------------------
        // Player info

        private int _ammo;
        public int Ammo => _ammo;

        private int _lives;
        public int Lives => _lives;

        private int _hearts;
        public int Hearts => _hearts;

        private int _coins;
        public int Coins => _coins;

        //--------------------------------------------------
        // Stored data

        public struct Data
        {
            public int Ammo;
            public int Lives;
            public int Hearts;
            public int Coins;
        }

        private Data _storedData;
        public Data StoredData => _storedData;

        //--------------------------------------------------
        // Initial data

        public const int InitialAmmo = 5;
        public const int InitialLives = 3;
        public const int InitialHearts = 5;

        //--------------------------------------------------
        // Stages completed

        private StageStatus[] _stagesCompleted;
        public StageStatus[] StagesCompleted => _stagesCompleted;

        //----------------------//------------------------//

        private PlayerManager()
        {
            _ammo = InitialAmmo;
            _lives = InitialLives;
            _hearts = InitialHearts;
            _coins = 100;
            _stagesCompleted = new StageStatus[SceneManager.MaxLevels];
            _storedData = new Data
            {
                Ammo = _ammo,
                Lives = _lives,
                Hearts = _hearts,
                Coins = _coins
            };
        }

        public void CreateNewGame()
        {
            SetData(InitialAmmo, InitialLives, InitialHearts, 0, new StageStatus[SceneManager.MaxLevels]);
        }

        public void SetData(int ammo, int lives, int hearts, int coins, StageStatus[] stagesCompleted)
        {
            _ammo = ammo;
            _lives = lives;
            _hearts = hearts;
            _coins = coins;
            _stagesCompleted = stagesCompleted;
        }

        public void StoreData()
        {
            _storedData = new Data
            {
                Ammo = _ammo,
                Lives = _lives,
                Hearts = _hearts,
                Coins = _coins
            };
        }

        public void RestoreSavedData()
        {
            _ammo = _storedData.Ammo;
            _lives = _storedData.Lives;
            _hearts = _storedData.Hearts;
            _coins = _storedData.Coins;
        }

        public void ResetHeartsAndLives()
        {
            _hearts = InitialHearts;
            _lives = InitialLives;
        }

        public void HandleRespawn()
        {
            _lives--;
            _hearts = InitialHearts;
        }

        public void AddAmmo(int amount)
        {
            _ammo += amount;
        }

        public void SetAmmo(int ammo)
        {
            _ammo = ammo;
        }

        public void AddLives(int amount)
        {
            _lives += amount;
        }

        public void AddHearts(int amount)
        {
            _hearts += amount;
        }

        public void AddCoins(int amount)
        {
            _coins += amount;
        }

        public void SetCoins(int coins)
        {
            _coins = coins;
        }

        public void CompleteStage(int mapId, bool rankS)
        {
            if (_stagesCompleted[mapId - 1].Completed)
            {
                if (rankS) _stagesCompleted[mapId - 1].RankS = true;
                return;
            }
            _stagesCompleted[mapId - 1].Completed = true;
            _stagesCompleted[mapId - 1].RankS = rankS;
        }

        public void CompleteAllStages()
        {
            for (var i = 0; i < SceneManager.MaxLevels; i++)
            {
                _stagesCompleted[i].Completed = true;
            }
        }
    }
}
