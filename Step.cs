using System;
using System.Collections.Generic;
using Robot.Common;
using Bot = Robot.Common.Robot;

namespace Orlov.Olexandr.RobotChallenge
{
    // 
    // Step
    //
    // Current step information and state of map at the moment,
    // methods for searching optimal destination, 
    // analyzing the state of current robot and possible interactions,
    // analyzing the state of stations and cells on the map
    //
    public class Step
    {
        public Map Map;
        public IList<Bot> RobotList;
        public int RobotIndex;
        public Bot Robot;
        public EnergyStation CurrentStation;

        public Step(int RobotIndex, IList<Bot> RobotList, Map Map)
        {
            this.RobotIndex = RobotIndex;
            this.RobotList  = RobotList;
            this.Map        = Map;

            SetRobot();
            SetCurrentStation();
        }

        public Position ClosestCollectingCell()
        {
            Position Destination = null;

            int MinAmount = int.MaxValue;

            foreach (EnergyStation Station in Map.Stations)
            {
                if ((Station == CurrentStation) || (IsConnectedEnough(Station, true)))
                    continue;

                Position Cell = FindFreeStationCell(Station);

                if (Cell == null)
                    continue;

                int Amount = DistanceHelper.ConsumptionForDistance(Cell, Robot.Position);

                if (Amount < MinAmount)
                {
                    MinAmount   = Amount;
                    Destination = Cell;
                }
            }

            return Destination;
        }

        public Position FindFreeStationCell(EnergyStation Station)
        {
            int MaxOffset        = Constants.ENERGY_COLLECTING_DISTANCE * 2,
                StepSignX        = (Station.Position.X > Robot.Position.X) ? 1 : -1,
                OffsetX          = Constants.ENERGY_COLLECTING_DISTANCE * -StepSignX,
                OptimalPositionX = Station.Position.X + OffsetX,
                StepSignY        = (Station.Position.Y > Robot.Position.Y) ? 1 : -1,
                OffsetY          = Constants.ENERGY_COLLECTING_DISTANCE * -StepSignY,
                OptimalPositionY = Station.Position.Y + OffsetY;

            for (int Counter = 0; Counter < MaxOffset; Counter++)
            {
                int DiagonalX = OptimalPositionX + Counter * StepSignX,
                    DiagonalY = OptimalPositionY;

                for (int DiagonalCounter = 0; DiagonalCounter <= Counter; DiagonalCounter++)
                {
                    DiagonalX += DiagonalCounter * -StepSignX;
                    DiagonalY += DiagonalCounter * StepSignY;

                    Position Cell = DistanceHelper.SavePoint(DiagonalX, DiagonalY, Map);

                    if (IsCellFree(Cell))
                        return Cell;     
                }
            }

            for (int Counter = 0; Counter <= MaxOffset; Counter++)
            {
                int DiagonalX = OptimalPositionX + MaxOffset * StepSignX,
                    DiagonalY = OptimalPositionY + Counter * StepSignY;

                for (int DiagonalCounter = 0; DiagonalCounter < MaxOffset - Counter; DiagonalCounter++)
                {
                    DiagonalX += DiagonalCounter * -StepSignX;
                    DiagonalY += DiagonalCounter * StepSignY;

                    Position Cell = DistanceHelper.SavePoint(DiagonalX, DiagonalY, Map);

                    if (IsCellFree(Cell))
                        return Cell;
                }
            }

            return null;
        }

        public Position AvailableDirectionalPosition(Position Point)
        {
            int FullAmount = DistanceHelper.ConsumptionForDistance(Robot.Position, Point);

            if (Robot.Energy >= FullAmount)
                return Point;

            int DistanceX = Math.Abs(Robot.Position.X - Point.X),
                DistanceY = Math.Abs(Robot.Position.Y - Point.Y),
                Parts = 2;

            while (Parts <= Math.Max(DistanceX, DistanceY))
            {
                int RequiredAmount = FullAmount / Parts,
                    StepSignX      = Robot.Position.X < Point.X ? 1 : -1,
                    StepSignY      = Robot.Position.Y < Point.Y ? 1 : -1,
                    X              = Robot.Position.X + StepSignX * DistanceX / Parts,
                    Y              = Robot.Position.Y + StepSignY * DistanceY / Parts;

                if (Robot.Position.X > Point.X)

                if (RequiredAmount * Parts < FullAmount)
                    RequiredAmount++;

                Position Spot = new Position(X, Y);

                if (RequiredAmount <= Robot.Energy)
                {
                    if (IsCellFree(Spot))
                        return Spot;
                    else
                    {
                        List<Position> Circle = DistanceHelper.OffsetCircle(Spot, Map, -StepSignX, -StepSignY);

                        foreach (Position NeigbourhoodCell in Circle)
                            if (IsCellFree(NeigbourhoodCell))
                                return NeigbourhoodCell;
                    }
                }

                Parts++;
            }

            return null;
        }

        public EnergyStation ConnectedStation(Bot SelectedRobot)
        {
            foreach (EnergyStation Station in Map.Stations)
                if (Math.Abs(Station.Position.X - SelectedRobot.Position.X) <= Constants.ENERGY_COLLECTING_DISTANCE &&
                    Math.Abs(Station.Position.Y - SelectedRobot.Position.Y) <= Constants.ENERGY_COLLECTING_DISTANCE)
                    return Station;

            return null;
        }

        public Bot ConnectedEnemy()
        {
            int Profit = 0;

            Bot Enemy  = null;

            foreach (Bot SelectedRobot in RobotList)
            {
                EnergyStation ClosestStation = ConnectedStation(SelectedRobot);

                if (SelectedRobot.Owner != Robot.Owner &&
                    ClosestStation == CurrentStation && 
                    SelectedRobot.Energy > Profit)
                {
                    Enemy  = SelectedRobot;
                    Profit = Enemy.Energy;
                }   
            }

            return Enemy;
        }

        public bool IsConnectedEnough(EnergyStation Station, bool IsOptionalDestination = false)
        {
            int RobotsCount = 0,
                Penalty     = IsOptionalDestination ? 1 : 0,
                Bonus       = 0;

            foreach (Bot SelectedRobot in RobotList)
            {
                EnergyStation ClosestStation = ConnectedStation(SelectedRobot);

                if (ClosestStation == Station)
                    RobotsCount++;
            }

            foreach (EnergyStation SelectedStation in Map.Stations)
                if (Math.Abs(SelectedStation.Position.X - Station.Position.X) <= Constants.ENERGY_COLLECTING_DISTANCE + 1 &&
                    Math.Abs(SelectedStation.Position.Y - Station.Position.Y) <= Constants.ENERGY_COLLECTING_DISTANCE + 1)
                    Bonus++;

            Bonus = (int)(Bonus * 0.5);

            return RobotsCount >= EnoughConnectedRobotsCount() - Penalty + Bonus;
        }

        public bool IsCellFree(Position Cell)
        {
            foreach (Bot SelectedRobot in RobotList)
                if (SelectedRobot.Position == Cell && SelectedRobot != Robot)
                    return false;

            return true;
        }

        public int RequiredEnergyForAttack(Bot Enemy)
        {
            return (Enemy != null) ?
                Constants.ENERGY_FOR_ATTACK + DistanceHelper.ConsumptionForDistance(Robot.Position, Enemy.Position) :
                int.MaxValue;
        }

        public bool IsCurrentStationUseless()
        {
            int PlayersCount   = Map.Stations.Count / (Constants.INITIAL_ROBOTS_COUNT * Constants.STATIONS_COUNT_MULTIPLIER),
                AcceptableLoss = PlayersCount < 5 ? 20 : 10;

            return CurrentStation.Energy < Constants.MAX_ENERGY_COLLECTED - AcceptableLoss;
        }

        private void SetRobot()
        {
            this.Robot = RobotList[RobotIndex];
        }

        private void SetCurrentStation()
        {
            this.CurrentStation = ConnectedStation(Robot);
        }

        private int EnoughConnectedRobotsCount()
        {
            int Count = Constants.MAX_STATION_GENERATED_ENERGY / Constants.MAX_ENERGY_COLLECTED;

            if (Constants.MAX_STATION_GENERATED_ENERGY > Count * Constants.MAX_ENERGY_COLLECTED)
                Count++;

            return Count;
        }
    }
}
