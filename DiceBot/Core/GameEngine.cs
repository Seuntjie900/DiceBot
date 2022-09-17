namespace DiceBot
{

    public enum GameEngineType
    {
        DICE,
        LIMBO,
        KENO,
        ROULETEE,
        HILO
    }

    public abstract class GameEngine
    {
        public GameEngineType GameType { get; private set; }

        public GameEngine(GameEngineType gameEngineType)
        {
            GameType = gameEngineType;
        }
    }


    public class KenoEngine : GameEngine
    {
        public KenoEngine() : base(GameEngineType.KENO)
        {

        }
    }

    public class LimboEngine : GameEngine
    {
        public LimboEngine() : base(GameEngineType.LIMBO)
        {

        }
    }

    public class DiceEngine : GameEngine
    {

        public DiceEngine() : base(GameEngineType.DICE)
        {
            //sqlite_helper.CheckDBS();
        }
    }


}
