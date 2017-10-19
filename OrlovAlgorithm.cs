using Robot.Common;
using System.Collections.Generic;
using Bot = Robot.Common.Robot;

namespace Orlov.Olexandr.RobotChallenge
{
    // 
    // OrlovAlgorithm
    //
    // Attributes with info about an algorithm,
    // round logger
    // method for performing the step according to selected strategy
    //
    public class OrlovAlgorithm : IRobotAlgorithm
    {
        public int Round { get; set; }

        public string Author
        {
            get
            {
                return "Orlov Alexandr";
            }
        }

        public string Description
        {
            get
            {
                return "Let's roll";
            }
        }

        public OrlovAlgorithm()
        {
            Logger.OnLogRound += Logger_OnLogRound;
        }

        private void Logger_OnLogRound(object sender, LogRoundEventArgs e)
        {
            Round++;
        }

        public RobotCommand DoStep(IList<Bot> robots, int robotToMoveIndex, Map map)
        {
            Step Step         = new Step(robotToMoveIndex, robots, map);
            Strategy Strategy = new Strategy(Step);

            switch (Strategy.Type)
            {
                case Strategy.MOVE_TYPE:
                    return Strategy.Move();
                case Strategy.CREATE_TYPE:
                    return Strategy.Create();
                case Strategy.ATTACK_TYPE:
                    return Strategy.Attack();
                case Strategy.COLLECT_TYPE:
                    return Strategy.Collect();
                default:
                    return null;
            }
        }       
    }
}
