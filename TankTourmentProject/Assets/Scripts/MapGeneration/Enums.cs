namespace MapGeneration
{
    public class Enums
    {
        public static Direction ReverseDirection(Direction direction)
        {
            /* Reverses the direction */
            switch (direction)
            {
                case Direction.Top:
                    return Direction.Bottom;
                case Direction.Bottom:
                    return Direction.Top;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                default:
                    return Direction.Top; // Will never happen, required for compilation
            }
        }
        
        public enum Direction
        {
            /* Enums of the Directions, used for the Wave function collapse */
            Top,
            Bottom,
            Left,
            Right
        }
    }
}
