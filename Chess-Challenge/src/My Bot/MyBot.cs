using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    readonly int[] pieceValues = { 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        (Move?, int) next = Search(board, 3, int.MinValue, int.MaxValue, true, board.IsWhiteToMove);
        if (next.Item1 != null) {
            return (Move)next.Item1;
        }
        return board.GetLegalMoves()[0];
    }

    private int Score(Board board, bool white) {
        if (board.IsInCheckmate()) return 1000000;
        int pieceScore = board.GetAllPieceLists()
            .Sum(list => pieceValues[(int)list.TypeOfPieceInList - 1] * list.Count * (list.IsWhitePieceList == white ? 1 : -1));
        return pieceScore;
    }

    private (Move?, int) Search(Board board, int depth, int alpha, int beta, bool isMax, bool white) {
        if (depth == 0) { return (null, Score(board, white)); }
        Random random = new();
        // randomise order
        Move[] moves = board.GetLegalMoves()
                .OrderByDescending(move => move.IsCapture ? pieceValues[(int)move.CapturePieceType - 1] : random.Next(50))
                .ToArray();
        if (moves.Length == 0) { return (null, Score(board, white)); }
        Move? bestMove = null;

        int value = isMax ? int.MinValue : int.MaxValue;
        bool better(int score) { return isMax ? score > value : score < value; }

        foreach (Move move in moves) {
            board.MakeMove(move);
            (Move?, int) next = Search(board, depth - 1, alpha, beta, !isMax, white);
            if (better(next.Item2)) {
                value = next.Item2;
                bestMove = move;
                if (isMax) {
                    alpha = Math.Max(alpha, value);
                    if (value >= beta) {
                        board.UndoMove(move);
                        break;
                    }
                } else {
                    beta = Math.Max(beta, value);
                    if (value >= alpha) {
                        board.UndoMove(move);
                        break;
                    }
                }
            }
            board.UndoMove(move);
        }
        return (bestMove, value);
    }
}
