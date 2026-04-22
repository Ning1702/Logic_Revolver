using Logic_Revolver.Core;
using System;
using System.Collections.Generic;

namespace Logic_Revolver.Game.Logic
{
    public class BuckshotAI_Clean
    {
        public string GetBestMove(GameState state)
        {
            // Sinh các hành động hợp lệ từ trạng thái hiện tại
            var moves = GetCandidateMoves(state);
            if (moves.Count == 0) return "WAIT";

            string bestMove = moves[0];
            double bestScore = double.NegativeInfinity;

            // Best-First đơn giản: chọn hành động có điểm cao nhất
            foreach (var move in moves)
            {
                GameState nextState = SimulateMove(state, move);
                double score = EvaluateTransition(state, move, nextState);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private List<string> GetCandidateMoves(GameState state)
        {
            var ai = state.Player2;
            var player = state.Player1;
            var moves = new List<string>();

            // Không còn đạn thì không có hành động tấn công
            if (state.LiveCount + state.BlankCount <= 0)
                return moves;

            // Tập toán tử hợp lệ
            if (ai.Inventory.Contains(ItemType.Glass) && state.KnownShell == null)
                moves.Add("USE_GLASS");

            if (ai.Inventory.Contains(ItemType.Cigarette) && ai.Hp < ai.MaxHp)
                moves.Add("USE_CIGARETTE");

            if (ai.Inventory.Contains(ItemType.Beer))
                moves.Add("USE_BEER");

            if (ai.Inventory.Contains(ItemType.Handcuffs) && !player.IsHandcuffed)
                moves.Add("USE_HANDCUFFS");

            if (ai.Inventory.Contains(ItemType.Knife) && state.DamageMultiplier == 1)
                moves.Add("USE_KNIFE");

            moves.Add("SHOOT_OPPONENT");
            moves.Add("SHOOT_SELF");

            return moves;
        }

        private double EvaluateTransition(GameState currentState, string move, GameState nextState)
        {
            // Hàm đánh giá tổng hợp = lợi ích nước đi + lợi thế trạng thái - đe dọa đối thủ
            double tacticalScore = EvaluateMoveUtility(currentState, move);
            double stateScore = EvaluateState(nextState);
            double opponentThreat = EstimateOpponentThreat(nextState);

            return tacticalScore + stateScore - opponentThreat;
        }

        private double EvaluateMoveUtility(GameState state, string move)
        {
            double pLive = GetPLive(state);
            var shell = GetShellInfo(state);

            var ai = state.Player2;
            var player = state.Player1;

            switch (move)
            {
                case "USE_GLASS":
                    // Heuristic thông tin
                    return shell.IsUnknown ? 7.0 : -2.0;

                case "USE_CIGARETTE":
                    // Heuristic sinh tồn
                    return 4.0 + (ai.MaxHp - ai.Hp) * 2.0;

                case "USE_BEER":
                    // Quyết định theo xác suất
                    if (shell.IsLive) return ai.Hp == 1 ? 4.0 : -1.0;
                    if (shell.IsBlank) return -3.0;
                    return 3.0 * pLive;

                case "USE_HANDCUFFS":
                    // Minimax rút gọn: giảm khả năng phản công của đối thủ
                    return shell.IsLive ? 7.0 : 3.0 + 2.0 * pLive;

                case "USE_KNIFE":
                    // Heuristic rủi ro - phần thưởng
                    if (shell.IsBlank) return -4.0;
                    if (shell.IsLive) return player.Hp <= 2 ? 10.0 : 6.0;
                    return (pLive >= 0.7 && player.Hp <= 2) ? 5.0 : 0.5;

                case "SHOOT_OPPONENT":
                    return EvaluateShootOpponent(state, pLive, shell);

                case "SHOOT_SELF":
                    return EvaluateShootSelf(state, pLive, shell);

                default:
                    return 0.0;
            }
        }

        private double EvaluateShootOpponent(GameState state, double pLive, ShellInfo shell)
        {
            // Utility-based: chấm theo sát thương kỳ vọng
            if (shell.IsBlank) return -6.0;

            if (shell.IsLive)
            {
                double value = 8.0 * state.DamageMultiplier;
                if (state.Player1.Hp <= state.DamageMultiplier)
                    value += 10.0; // thưởng kết liễu
                return value;
            }

            double expectedDamage = pLive * state.DamageMultiplier;
            double finishBonus = expectedDamage >= state.Player1.Hp ? 8.0 : 0.0;
            double riskPenalty = (1.0 - pLive) * 3.0;

            return expectedDamage * 8.0 + finishBonus - riskPenalty;
        }

        private double EvaluateShootSelf(GameState state, double pLive, ShellInfo shell)
        {
            // Ra quyết định dưới bất định
            if (shell.IsBlank) return 9.0;
            if (shell.IsLive) return -12.0 * state.DamageMultiplier;

            double keepTurnReward = (1.0 - pLive) * 8.0;
            double selfDamageRisk = pLive * 10.0 * state.DamageMultiplier;

            return keepTurnReward - selfDamageRisk;
        }

        private double EvaluateState(GameState state)
        {
            var ai = state.Player2;
            var player = state.Player1;
            double pLive = GetPLive(state);

            double score = 0.0;

            // Heuristic lợi thế tài nguyên
            score += ai.Hp * 6.0 - player.Hp * 6.0;
            score += ai.Inventory.Count * 1.5 - player.Inventory.Count * 1.2;

            // Heuristic kiểm soát lượt
            if (player.IsHandcuffed)
                score += 5.0;

            // Heuristic thông tin
            if (state.KnownShell == ShellType.Live)
                score += 4.0;
            else if (state.KnownShell == ShellType.Blank)
                score += 3.0;

            // Heuristic tấn công
            score += (state.DamageMultiplier - 1) * 3.0;

            // Heuristic phòng thủ
            if (ai.Hp == 1)
                score -= pLive * 5.0;

            return score;
        }

        private double EstimateOpponentThreat(GameState state)
        {
            // Minimax rút gọn 1 lớp: ước lượng phản công của đối thủ
            var player = state.Player1;
            double pLive = GetPLive(state);

            double threat = 0.0;
            threat += player.Hp * 1.5;
            threat += player.Inventory.Count * 1.0;
            threat += pLive * state.DamageMultiplier * 4.0;

            if (player.IsHandcuffed)
                threat -= 5.0;

            return Math.Max(0.0, threat);
        }

        private double GetPLive(GameState state)
        {
            // Suy luận xác suất viên hiện tại là đạn thật
            int total = state.LiveCount + state.BlankCount;
            if (total <= 0) return 0.0;

            if (state.KnownShell == ShellType.Live) return 1.0;
            if (state.KnownShell == ShellType.Blank) return 0.0;

            return (double)state.LiveCount / total;
        }

        private GameState SimulateMove(GameState state, string move)
        {
            // Mô phỏng chuyển trạng thái
            var next = CloneState(state);
            var ai = next.Player2;
            var player = next.Player1;

            switch (move)
            {
                case "USE_GLASS":
                    // Bản đơn giản: chỉ coi như hành động lấy thông tin
                    break;

                case "USE_CIGARETTE":
                    if (ai.Inventory.Contains(ItemType.Cigarette) && ai.Hp < ai.MaxHp)
                    {
                        ai.Hp = Math.Min(ai.Hp + 1, ai.MaxHp);
                        ai.Inventory.Remove(ItemType.Cigarette);
                    }
                    break;

                case "USE_BEER":
                    if (ai.Inventory.Contains(ItemType.Beer))
                    {
                        RemoveCurrentShell(next);
                        ai.Inventory.Remove(ItemType.Beer);
                        next.KnownShell = null;
                    }
                    break;

                case "USE_HANDCUFFS":
                    if (ai.Inventory.Contains(ItemType.Handcuffs) && !player.IsHandcuffed)
                    {
                        player.IsHandcuffed = true;
                        ai.Inventory.Remove(ItemType.Handcuffs);
                    }
                    break;

                case "USE_KNIFE":
                    if (ai.Inventory.Contains(ItemType.Knife) && next.DamageMultiplier == 1)
                    {
                        next.DamageMultiplier = 2;
                        ai.Inventory.Remove(ItemType.Knife);
                    }
                    break;

                case "SHOOT_OPPONENT":
                    ApplyShot(next, true);
                    break;

                case "SHOOT_SELF":
                    ApplyShot(next, false);
                    break;
            }

            return next;
        }

        private void ApplyShot(GameState state, bool shootOpponent)
        {
            // Hàm chuyển trạng thái sau khi bắn
            var ai = state.Player2;
            var player = state.Player1;

            bool isLive = ResolveCurrentShellIsLive(state);
            bool isBlank = !isLive;

            if (isLive)
            {
                if (shootOpponent) player.Hp -= state.DamageMultiplier;
                else ai.Hp -= state.DamageMultiplier;

                if (state.LiveCount > 0) state.LiveCount--;
            }
            else if (isBlank)
            {
                if (state.BlankCount > 0) state.BlankCount--;
            }

            state.DamageMultiplier = 1;
            state.KnownShell = null;
        }

        private void RemoveCurrentShell(GameState state)
        {
            // Loại bỏ viên hiện tại khỏi ổ đạn
            if (state.KnownShell == ShellType.Live)
            {
                if (state.LiveCount > 0) state.LiveCount--;
                return;
            }

            if (state.KnownShell == ShellType.Blank)
            {
                if (state.BlankCount > 0) state.BlankCount--;
                return;
            }

            if (ResolveCurrentShellIsLive(state))
            {
                if (state.LiveCount > 0) state.LiveCount--;
            }
            else
            {
                if (state.BlankCount > 0) state.BlankCount--;
            }
        }

        private bool ResolveCurrentShellIsLive(GameState state)
        {
            // Nếu chưa biết, xấp xỉ theo xác suất
            if (state.KnownShell == ShellType.Live) return true;
            if (state.KnownShell == ShellType.Blank) return false;
            return GetPLive(state) >= 0.5;
        }

        private ShellInfo GetShellInfo(GameState state)
        {
            return new ShellInfo
            {
                IsLive = state.KnownShell == ShellType.Live,
                IsBlank = state.KnownShell == ShellType.Blank,
                IsUnknown = state.KnownShell == null
            };
        }

        private GameState CloneState(GameState state)
        {
            // Sao chép trạng thái để mô phỏng
            return new GameState
            {
                LiveCount = state.LiveCount,
                BlankCount = state.BlankCount,
                KnownShell = state.KnownShell,
                DamageMultiplier = state.DamageMultiplier,
                Player1 = new Player(state.Player1.Name, state.Player1.MaxHp, state.Player1.IsAi)
                {
                    Hp = state.Player1.Hp,
                    IsHandcuffed = state.Player1.IsHandcuffed,
                    Inventory = new List<ItemType>(state.Player1.Inventory)
                },
                Player2 = new Player(state.Player2.Name, state.Player2.MaxHp, state.Player2.IsAi)
                {
                    Hp = state.Player2.Hp,
                    IsHandcuffed = state.Player2.IsHandcuffed,
                    Inventory = new List<ItemType>(state.Player2.Inventory)
                }
            };
        }

        private struct ShellInfo
        {
            public bool IsLive;
            public bool IsBlank;
            public bool IsUnknown;
        }
    }
}