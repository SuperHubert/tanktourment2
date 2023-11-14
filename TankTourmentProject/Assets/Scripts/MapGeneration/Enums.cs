using System.Collections.Generic;

namespace MapGeneration
{
    public class Enums
    {
        public static List<TileType> AllTileTypes = new List<TileType>()
        {
            TileType.Cross,
            TileType.LineHorizontal,
            TileType.LineVertical,
            TileType.LShapeTL,
            TileType.LShapeTR,
            TileType.LShapeBL,
            TileType.LShapeBR,
        };
        
        
        public static NeighborType GetNeighborTypeByDirection(TileType tileType, Direction direction)
        {
            switch (tileType)
            {
                case TileType.Cross:
                    return GetNeighborCoss(direction);
                case TileType.LineHorizontal:
                    return GetNeighborLineHorizontal(direction);
                case TileType.LineVertical:
                    return GetNeighborLineVertical(direction); 
                case TileType.LShapeTL:
                    return GetNeighborLShapeTL(direction);
                case TileType.LShapeTR:
                    return GetNeighborLShapeTR(direction);
                case TileType.LShapeBL:
                    return GetNeighborLShapeBL(direction);
                case TileType.LShapeBR:
                    return GetNeighborLShapeBR(direction);
            }
            
            return NeighborType.Closed; // Will never happen, required for compilation
        }

        private static NeighborType GetNeighborCoss(Direction direction)
        {
            return NeighborType.Open;
        }
        
        private static NeighborType GetNeighborLineHorizontal(Direction direction)
        {
            switch (direction)
            {
                case Direction.Top:
                    return NeighborType.Closed;
                case Direction.Bottom:
                    return NeighborType.Closed;
                default:
                    return NeighborType.Open;
            }
        }
        
        private static NeighborType GetNeighborLineVertical(Direction direction)
        {
            switch (direction)
            {
                case Direction.Top:
                    return NeighborType.Open;
                case Direction.Bottom:
                    return NeighborType.Open;
                default:
                    return NeighborType.Closed;
            }
        }

        private static NeighborType GetNeighborLShapeTL(Direction direction)
        {
            switch (direction)
            {
                case Direction.Top:
                    return NeighborType.Open;
                case Direction.Left:
                    return NeighborType.Open;
                default:
                    return NeighborType.Closed;
            }
        }
        
        private static NeighborType GetNeighborLShapeTR(Direction direction)
        {
            switch (direction)
            {
                case Direction.Top:
                    return NeighborType.Open;
                case Direction.Right:
                    return NeighborType.Open;
                default:
                    return NeighborType.Closed;
            }
        }
        
        private static NeighborType GetNeighborLShapeBL(Direction direction)
        {
            switch (direction)
            {
                case Direction.Bottom:
                    return NeighborType.Open;
                case Direction.Left:
                    return NeighborType.Open;
                default:
                    return NeighborType.Closed;
            }
        }
        
        private static NeighborType GetNeighborLShapeBR(Direction direction)
        {
            switch (direction)
            {
                case Direction.Bottom:
                    return NeighborType.Open;
                case Direction.Right:
                    return NeighborType.Open;
                default:
                    return NeighborType.Closed;
            }
        }


        
        
        
        public enum Direction
        {
            Top,
            Bottom,
            Left,
            Right
        }

        public enum TileType
        {
            None,
            Cross,
            LineHorizontal,
            LineVertical,
            LShapeTL,
            LShapeTR,
            LShapeBL,
            LShapeBR,
        }

        public enum NeighborType
        {
            Open,
            Closed,

        }
    }
}
