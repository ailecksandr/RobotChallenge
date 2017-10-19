using Robot.Common;
using System.Linq;
using Bot = Robot.Common.Robot;

namespace Orlov.Olexandr.RobotChallenge
{
    // 
    // Strategy
    //
    // Methods for analyzing better strategy for current step
    // Receives optimal type as constant after initializing
    //
    public class Strategy
    {
        public const string ATTACK_TYPE  = "ATTACK";
        public const string MOVE_TYPE    = "MOVE";
        public const string COLLECT_TYPE = "COLLECT";
        public const string CREATE_TYPE  = "CREATE";

        public Step Step;
        public string Type;

        public Strategy(Step Step)
        {
            this.Step = Step;

            Categorize();
        }

        public MoveCommand Move()
        {
            Position CollectingCell = Step.ClosestCollectingCell();

            if (CollectingCell == null)
                return null;

            Position DirectionalPosition = Step.AvailableDirectionalPosition(CollectingCell);

            return new MoveCommand() { NewPosition = DirectionalPosition };
        }

        public RobotCommand Create()
        {
            return new CreateNewRobotCommand();
        }

        public MoveCommand Attack()
        {
            Bot PrioritizedEnemy = Step.ConnectedEnemy();

            return new MoveCommand() { NewPosition = PrioritizedEnemy.Position };
        }

        public CollectEnergyCommand Collect()
        {
            return new CollectEnergyCommand();
        }

        public bool CanCreate()
        {
            int SafeAmount          = Constants.ENERGY_FOR_NEW_ROBOT + Constants.ENERGY_FOR_CREATING,
                FriendlyRobotsCount = Step.RobotList.Where(SelectedRobot => SelectedRobot.Owner == Step.Robot.Owner).ToList().Count;

            return Step.Robot.Energy > SafeAmount &&
                FriendlyRobotsCount < Constants.MAX_ROBOTS_COUNT &&
                !Step.IsConnectedEnough(Step.CurrentStation);
        }

        public bool CanAttack(Bot Enemy)
        {
            if (Enemy == null)
                return false;

            int RequiredAmount = Step.RequiredEnergyForAttack(Enemy);

            return Step.Robot.Energy >= RequiredAmount &&
                Step.CurrentStation.Energy <= Enemy.Energy * Constants.ENERGY_STEALING_COEFFICIENT - RequiredAmount &&
                Step.IsConnectedEnough(Step.CurrentStation);
        }

        public bool CanCollect()
        {
            return !(Step.IsCurrentStationUseless() &&
                Step.Robot.Energy > Constants.MAX_ENERGY_COLLECTED);
        }

        private string Categorize()
        {
            if (Step.CurrentStation == null)
                return this.Type = MOVE_TYPE;

            if (CanCreate())
                return this.Type = CREATE_TYPE;

            Bot PrioritizedEnemy = Step.ConnectedEnemy();

            if (CanAttack(PrioritizedEnemy))
                return this.Type = ATTACK_TYPE;

            if (CanCollect())
                return this.Type = COLLECT_TYPE;

            return this.Type = MOVE_TYPE;
        }
    }
}
