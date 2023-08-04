using ChessChallenge.API;
using System;
using System.Linq;

delegate int PieceLocationModifier(Square square);

public class MyBot : IChessBot
{
    // null, pawn, knight, bishop, rook, queen, king
    readonly int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    readonly PieceLocationModifier[] modifiers = {
        // null
        s => 0,
        // pawn
        s => s.Rank > 3 ? 30 : (s.Rank == 2 && s.File > 2 && s.File < 5 ? 20 : 0),
        // knight
        s => s.Rank == 0 || s.Rank == 7 || s.File == 0 || s.File == 7 ? -40 : (4 - Math.Abs(s.Rank - 4) + (4 - Math.Abs(s.Index - 4)))*10,
        // bishop
        s => s.Rank == 0 || s.Rank == 7 || s.File == 0 || s.File == 7 ? -10 : 1,
        // rook
        s => s.Rank == 0 || s.File == 0 || s.File == 7 ? -10 : s.File == 6 ? 10 : 0,
        // queen
        s => s.Rank == 0 || s.Rank == 7 || s.File == 0 || s.File == 7 ? -10 : 0,
        // king
        s => s.Rank > 2 ? -20 : s.Index < 3 || s.Index > 5 ? 20 : 0,
    };

    public Move Think(Board board, Timer timer)
    {
        (Move?, int) next = Search(board, 5, int.MinValue, int.MaxValue, true, board.IsWhiteToMove);
        if (next.Item1 != null)
        {
            return (Move)next.Item1;
        }
        return board.GetLegalMoves()[0];
    }

    private int Score(Board board, bool white)
    {
        if (board.IsInCheckmate()) return 1000000;
        if (board.IsDraw()) return 0;
        int pieceScore = board.GetAllPieceLists()
            .Sum(list => ((pieceValues[(int)list.TypeOfPieceInList] * list.Count) +
                list.Sum(piece => modifiers[(int)piece.PieceType](piece.Square))) * (list.IsWhitePieceList == white ? 1 : -1)
            );

        int inCheckMod = board.IsInCheck() ? 400 : 0;
        return pieceScore + inCheckMod;
    }

    private (Move?, int) Search(Board board, int depth, int alpha, int beta, bool isMax, bool white)
    {
        if (depth == 0) { return (null, Score(board, white)); }
        Random random = new();
        // randomise order
        Move[] moves = board.GetLegalMoves()
                .OrderByDescending(move => move.IsCapture ? pieceValues[(int)move.CapturePieceType] : random.Next(5))
                .ToArray();
        if (moves.Length == 0) { return (null, Score(board, white)); }
        Move? bestMove = null;

        int value = isMax ? int.MinValue : int.MaxValue;
        bool better(int score) { return isMax ? score > value : score < value; }

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            (Move?, int) next = Search(board, depth - 1, alpha, beta, !isMax, white);
            bool doBreak = false;
            if (better(next.Item2))
            {
                value = next.Item2;
                bestMove = move;
                if (isMax)
                {
                    doBreak = value >= beta;
                    alpha = Math.Max(alpha, value);
                }
                else
                {
                    doBreak = value <= alpha;
                    beta = Math.Min(beta, value);
                }
            }
            board.UndoMove(move);
            if (doBreak) { break; }
        }
        return (bestMove, value);
    }
}
