namespace Clase15
{
    public struct PlayerInfo
    {
        public string playerName;
        public int playerHP;
        public int playerMana;
        public float playerSpeed;
        public PlayerInfo(string _playerName, int _playerHP, int _playerMana, float _playerSpeed)
        {
            playerName = _playerName;
            playerHP = _playerHP;
            playerMana = _playerMana;
            playerSpeed = _playerSpeed;
        }
    }
}