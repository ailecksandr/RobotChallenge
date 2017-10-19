using System;
using Robot.Common;
using System.Collections.Generic;

namespace Orlov.Olexandr.RobotChallenge
{
    // 
    // DistanceHelper
    //
    // Methods with Position manipulation
    //
    public class DistanceHelper
    {
        public static int ConsumptionForDistance(Position PointA, Position PointB)
        {
            return (int)(Math.Pow(PointA.X - PointB.X, 2) + Math.Pow(PointA.Y - PointB.Y, 2));
        }

        public static Position SavePoint(int X, int Y, Map Map)
        {
            if (X >= Map.MaxPozition.X)
                X = Map.MaxPozition.X;
            if (X <= Map.MinPozition.X)
                X = Map.MinPozition.X;

            if (Y >= Map.MaxPozition.Y)
                Y = Map.MaxPozition.Y;
            if (Y <= Map.MinPozition.Y)
                Y = Map.MinPozition.Y;

            return new Position(X, Y);
        }

        public static List<Position> OffsetCircle(Position Point, Map Map, int StartOffsetX = 1, int StartOffsetY = 1)
        {
            List<Position> Circle = new List<Position>();

            int OffsetX    = StartOffsetX,
                OffsetY    = StartOffsetY,
                MoveSignX  = -StartOffsetX,
                MoveSignY  = -StartOffsetY,
                MovesCount = 0;

            bool IsMovedByX = true;

            do
            {
                Position Spot = SavePoint(Point.X + OffsetX, Point.Y + OffsetY, Map);

                Circle.Add(Spot);
                MovesCount++;

                if (IsMovedByX)
                {
                    OffsetX += MoveSignX;

                    if (MovesCount == 2)
                    {
                        IsMovedByX = false;
                        MovesCount = 0;
                        MoveSignX = -MoveSignX;
                    }
                }
                else
                {
                    OffsetY += MoveSignY;

                    if (MovesCount == 2)
                    {
                        IsMovedByX = true;
                        MovesCount = 0;
                        MoveSignY  = -MoveSignY;
                    }
                }
            } while (OffsetX != StartOffsetX || OffsetY != StartOffsetY);

            return Circle;
        }
    }
}
