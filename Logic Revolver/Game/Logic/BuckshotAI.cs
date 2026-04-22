using Logic_Revolver.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logic_Revolver.Game.Logic
{
    public class BuckshotAI_Clean
    {
        public string GetBestMove(GameState state)
        {
            var moves = GetCandidateMoves(state);
            if (moves.Count == 0) return "WAIT";

            string bestMove = moves[0];
            double bestScore = double.NegativeInfinity;

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
            bool knownLive = state.KnownShell == ShellType.Live;
            bool knownBlank = state.KnownShell == ShellType.Blank;

            var ai = state.Player2;
            var player = state.Player1;

            switch (move)
            {
                case "USE_GLASS":
                    return state.KnownShell == null ? 7.0 : -2.0;

                case "USE_CIGARETTE":
                    return 4.0 + (ai.MaxHp - ai.Hp) * 2.0;

                case "USE_BEER":
                    if (knownLive) return ai.Hp == 1 ? 4.0 : -1.0;
                    if (knownBlank) return -3.0;
                    return 3.0 * pLive;

                case "USE_HANDCUFFS":
                    return knownLive ? 7.0 : 3.0 + 2.0 * pLive;

                case "USE_KNIFE":
                    if (knownBlank) return -4.0;
                    if (knownLive) return player.Hp <= 2 ? 10.0 : 6.0;
                    return (pLive >= 0.7 && player.Hp <= 2) ? 5.0 : 0.5;

                case "SHOOT_OPPONENT":
                    return EvaluateShootOpponent(state, pLive);

                case "SHOOT_SELF":
                    return EvaluateShootSelf(state, pLive);

                default:
                    return 0.0;
            }
        }

        private double EvaluateShootOpponent(GameState state, double pLive)
        {
            if (state.KnownShell == ShellType.Blank)
                return -6.0;

            if (state.KnownShell == ShellType.Live)
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

        private double EvaluateShootSelf(GameState state, double pLive)
        {
            if (state.KnownShell == ShellType.Blank)
                return 9.0;

            if (state.KnownShell == ShellType.Live)
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

            // 1. Máu
            score += ai.Hp * 6.0;
            score -= player.Hp * 6.0;

            // 2. Vật phẩm
            score += ai.Inventory.Count * 1.5;
            score -= player.Inventory.Count * 1.2;

            // 3. Kiểm soát lượt / khóa đối thủ
            if (player.IsHandcuffed)
                score += 5.0;

            // 4. Kiến thức về viên đạn hiện tại
            if (state.KnownShell == ShellType.Live)
                score += 4.0;
            else if (state.KnownShell == ShellType.Blank)
                score += 3.0;

            // 5. Damage multiplier
            score += (state.DamageMultiplier - 1) * 3.0;

            // 6. Rủi ro nếu AI đang rất yếu
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
                    break;

                case "USE_CIGARETTE":
                    if (ai.Inventory.Contains(ItemType.Cigarette) && ai.Hp < ai.MaxHp)
                    {
                        ai.Hp++;
                        if (ai.Hp > ai.MaxHp) ai.Hp = ai.MaxHp;
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

            bool isLive = state.KnownShell == ShellType.Live;
            bool isBlank = state.KnownShell == ShellType.Blank;

            if (state.KnownShell == null)
            {
                double pLive = GetPLive(state);
                isLive = pLive >= 0.5;
                isBlank = !isLive;
            }

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
            if (state.KnownShell == ShellType.Live)
            {
                if (state.LiveCount > 0) state.LiveCount--;
            }
            else if (state.KnownShell == ShellType.Blank)
            {
                if (state.BlankCount > 0) state.BlankCount--;
            }
            else
            {
                double pLive = GetPLive(state);
                if (pLive >= 0.5 && state.LiveCount > 0)
                    state.LiveCount--;
                else if (state.BlankCount > 0)
                    state.BlankCount--;
            }
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
    }
}