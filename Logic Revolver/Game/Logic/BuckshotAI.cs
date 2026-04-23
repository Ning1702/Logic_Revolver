using Logic_Revolver.Core;
using System;
using System.Collections.Generic;

namespace Logic_Revolver.Game.Logic
{
    public class BuckshotAI
    {
        public string GetBestMove(GameState state)
        {
            var ai = state.Player2;
            var player = state.Player1;

            int live = state.LiveCount;
            int blank = state.BlankCount;
            int total = live + blank;
            double pLive = GetPLive(state);

            if (total == 0) return "WAIT";

            int itemCount = ai.Inventory.Count;
            bool shellUnknown = state.KnownShell == null;
            bool enemyWeak = player.Hp <= 2;
            bool aiInDanger = ai.Hp <= 2;

            // --- GIAI ĐOẠN 1: DÙNG ITEM THÔNG TIN / PHÒNG THỦ CHỦ ĐỘNG HƠN ---

            // Kính lúp: dùng sớm hơn nếu chưa biết viên hiện tại
            if (ai.Inventory.Contains(ItemType.Glass) && shellUnknown)
            {
                if (total >= 2 || itemCount >= 2 || aiInDanger || enemyWeak)
                    return "USE_GLASS";
            }

            // Hồi máu khi nguy hiểm hoặc khi xác suất đạn thật khá cao
            if (ai.Inventory.Contains(ItemType.Cigarette) &&
                ai.Hp < ai.MaxHp &&
                (ai.Hp <= 2 || pLive >= 0.55))
            {
                return "USE_CIGARETTE";
            }

            // --- GIAI ĐOẠN 2: KHI ĐÃ BIẾT SHELL HIỆN TẠI ---

            if (state.KnownShell != null)
            {
                if (state.KnownShell == ShellType.Live)
                {
                    if (ai.Inventory.Contains(ItemType.Knife) &&
                        state.DamageMultiplier == 1 &&
                        player.Hp <= 2)
                    {
                        return "USE_KNIFE";
                    }

                    if (ai.Inventory.Contains(ItemType.Handcuffs) &&
                        !player.IsHandcuffed)
                    {
                        return "USE_HANDCUFFS";
                    }

                    if (ai.Inventory.Contains(ItemType.Knife) &&
                        state.DamageMultiplier == 1)
                    {
                        return "USE_KNIFE";
                    }

                    if (ai.Inventory.Contains(ItemType.Beer) &&
                        ai.Hp == 1 &&
                        player.Hp > state.DamageMultiplier)
                    {
                        return "USE_BEER";
                    }

                    return "SHOOT_OPPONENT";
                }
                else
                {
                    // Biết chắc blank: bắn mình để tối ưu nhịp
                    return "SHOOT_SELF";
                }
            }

            // --- GIAI ĐOẠN 3: CHƯA BIẾT SHELL, DÙNG ITEM CHỦ ĐỘNG HƠN ---

            if (ai.Inventory.Contains(ItemType.Beer))
            {
                bool shouldUseBeer =
                    pLive >= 0.60 ||
                    (ai.Hp == 1 && pLive >= 0.45) ||
                    (itemCount >= 3 && total >= 2 && pLive >= 0.50);

                if (shouldUseBeer)
                    return "USE_BEER";
            }

            if (ai.Inventory.Contains(ItemType.Handcuffs) &&
                !player.IsHandcuffed &&
                pLive >= 0.55)
            {
                return "USE_HANDCUFFS";
            }

            if (ai.Inventory.Contains(ItemType.Knife) &&
                state.DamageMultiplier == 1 &&
                ((pLive >= 0.65 && player.Hp <= 2) ||
                 (pLive >= 0.75 && itemCount >= 2)))
            {
                return "USE_KNIFE";
            }

            // --- GIAI ĐOẠN 4: CHẤM ĐIỂM TOÀN BỘ HÀNH ĐỘNG ---

            var candidateMoves = GetCandidateMoves(state);

            if (candidateMoves.Count == 0)
                return "WAIT";

            string bestMove = candidateMoves[0];
            double bestScore = double.NegativeInfinity;

            foreach (var move in candidateMoves)
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

            if (state.LiveCount + state.BlankCount <= 0)
                return moves;

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

            int currentItemCount = ai.Inventory.Count;
            int totalShells = state.LiveCount + state.BlankCount;

            double score = 0.0;

            switch (move)
            {
                case "USE_GLASS":
                    score += shell.IsUnknown ? 8.5 : -3.0;

                    if (totalShells >= 2) score += 2.5;
                    if (currentItemCount >= 2) score += 2.0;
                    if (ai.Hp <= 2) score += 1.5;
                    if (player.Hp <= 2) score += 1.0;
                    break;

                case "USE_CIGARETTE":
                    score += 5.0 + (ai.MaxHp - ai.Hp) * 1.5;
                    if (ai.Hp == 1) score += 3.0;
                    break;

                case "USE_BEER":
                    if (shell.IsLive)
                    {
                        score += (ai.Hp == 1 && player.Hp > state.DamageMultiplier) ? 4.0 : 1.0;
                    }
                    else if (shell.IsBlank)
                    {
                        score -= 3.0;
                    }
                    else
                    {
                        if (pLive >= 0.7) score += 4.5;
                        else if (pLive >= 0.55) score += 2.5;
                        else score += 0.5;

                        if (ai.Hp == 1) score += 2.0;
                        if (currentItemCount >= 3) score += 1.5;
                        if (totalShells >= 2) score += 1.0;
                    }

                    score += 1.5;
                    break;

                case "USE_HANDCUFFS":
                    score += !player.IsHandcuffed ? 5.0 : -3.0;
                    if (shell.IsLive) score += 3.0;
                    else if (!shell.IsBlank && pLive >= 0.55) score += 2.0;
                    score += 2.0;
                    break;

                case "USE_KNIFE":
                    if (state.DamageMultiplier > 1)
                    {
                        score -= 4.0;
                    }
                    else if (shell.IsBlank)
                    {
                        score -= 5.0;
                    }
                    else if (shell.IsLive)
                    {
                        score += 7.0;
                        if (player.Hp <= 2) score += 4.0;
                    }
                    else
                    {
                        if (pLive >= 0.7 && player.Hp <= 2) score += 4.0;
                        else if (pLive >= 0.75) score += 2.0;
                        else score -= 1.0;
                    }

                    score += 1.5;
                    break;

                case "SHOOT_OPPONENT":
                    score += EvaluateShootOpponent(state, pLive, shell);
                    break;

                case "SHOOT_SELF":
                    score += EvaluateShootSelf(state, pLive, shell);
                    break;
            }

            // Phạt nhẹ nếu còn nhiều item mà vẫn không dùng
            if ((move == "SHOOT_OPPONENT" || move == "SHOOT_SELF") &&
                currentItemCount >= 3 &&
                totalShells >= 2)
            {
                score -= 1.5;
            }

            return score;
        }

        private double EvaluateShootOpponent(GameState state, double pLive, ShellInfo shell)
        {
            if (shell.IsBlank)
                return -6.0;

            if (shell.IsLive)
            {
                double value = 8.0 * state.DamageMultiplier;
                if (state.Player1.Hp <= state.DamageMultiplier)
                    value += 10.0;
                return value;
            }

            double expectedDamage = pLive * state.DamageMultiplier;
            double finishBonus = expectedDamage >= state.Player1.Hp ? 8.0 : 0.0;
            double riskPenalty = (1.0 - pLive) * 3.0;

            return expectedDamage * 8.0 + finishBonus - riskPenalty;
        }

        private double EvaluateShootSelf(GameState state, double pLive, ShellInfo shell)
        {
            if (shell.IsBlank)
                return 9.0;

            if (shell.IsLive)
                return -12.0 * state.DamageMultiplier;

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

            // Máu
            score += ai.Hp * 6.0;
            score -= player.Hp * 6.0;

            // Vật phẩm
            score += ai.Inventory.Count * 1.5;
            score -= player.Inventory.Count * 1.2;

            // Khóa đối thủ
            if (player.IsHandcuffed)
                score += 5.0;

            // Biết thông tin shell hiện tại
            if (state.KnownShell == ShellType.Live)
                score += 4.0;
            else if (state.KnownShell == ShellType.Blank)
                score += 3.0;

            // Buff damage
            score += (state.DamageMultiplier - 1) * 3.0;

            // Nếu AI đang rất yếu thì state nguy hiểm hơn khi pLive cao
            if (ai.Hp == 1)
                score -= pLive * 5.0;

            return score;
        }

        private double EstimateOpponentThreat(GameState state)
        {
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
            int total = state.LiveCount + state.BlankCount;
            if (total <= 0) return 0.0;

            if (state.KnownShell == ShellType.Live) return 1.0;
            if (state.KnownShell == ShellType.Blank) return 0.0;

            return (double)state.LiveCount / total;
        }

        private GameState SimulateMove(GameState state, string move)
        {
            var next = CloneState(state);
            var ai = next.Player2;
            var player = next.Player1;

            switch (move)
            {
                case "USE_GLASS":
                    if (ai.Inventory.Contains(ItemType.Glass))
                    {
                        ai.Inventory.Remove(ItemType.Glass);

                        // Nếu engine của bạn có cơ chế soi viên hiện tại,
                        // hãy gán next.KnownShell ở đây.
                    }
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
            var ai = state.Player2;
            var player = state.Player1;

            bool isLive = ResolveCurrentShellIsLive(state);
            bool isBlank = !isLive;

            if (isLive)
            {
                if (shootOpponent)
                    player.Hp -= state.DamageMultiplier;
                else
                    ai.Hp -= state.DamageMultiplier;

                if (state.LiveCount > 0)
                    state.LiveCount--;
            }
            else if (isBlank)
            {
                if (state.BlankCount > 0)
                    state.BlankCount--;
            }

            state.DamageMultiplier = 1;
            state.KnownShell = null;
        }

        private void RemoveCurrentShell(GameState state)
        {
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