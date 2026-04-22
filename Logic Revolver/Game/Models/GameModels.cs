using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic_Revolver.Core
{

    // 1. Các định nghĩa cơ bản
    public enum ShellType { Blank, Live }
    public enum ItemType { Beer, Knife, Glass, Handcuffs, Cigarette }
    public enum Target { Self, Opponent } 

    // 2. Class Player
    public class Player
    {
        public string Name { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public bool IsAi { get; set; }
        public bool IsHandcuffed { get; set; } = false;
        public List<ItemType> Inventory { get; set; } = new List<ItemType>();

        public Player(string name, int maxHp, bool isAi)
        {
            Name = name;
            Hp = maxHp;
            MaxHp = maxHp;
            IsAi = isAi;
        }
    }

    // 3. Class GameState (Lưu trữ dữ liệu bàn cờ)
    public class GameState
    {
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public Player CurrentPlayer { get; set; }
        public int LiveCount { get; set; }
        public int BlankCount { get; set; }
        public ShellType? KnownShell { get; set; } = null;
        public int DamageMultiplier { get; set; } = 1;

        public Player GetOpponent(Player p)
        {
            return p == Player1 ? Player2 : Player1;
        }
    }

}
