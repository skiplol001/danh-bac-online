using System;
using System.Data;
using System.Data.SqlClient;

namespace gamethebaiv2.Game
{
    public static class MonsterActionService
    {
        // Dùng một đối tượng Random duy nhất để tránh trùng lặp seed khi gọi liên tục
        private static readonly Random _rand = new Random();

        public static string Execute(DataRow row, CombatEntity monster, CombatEntity hero, int runID, string strConn)
        {
            // 1. Lấy dữ liệu thô từ Database
            string type = row["MaLoai"].ToString();
            string actionName = row["TenHanhDong"].ToString();
            int vMin = Convert.ToInt32(row["GiaTriMin"]);
            int vMax = Convert.ToInt32(row["GiaTriMax"]);
            int turns = Convert.ToInt32(row["SoLuot"]);

            // 2. Tính toán giá trị ngẫu nhiên trong khoảng Min - Max
            int val = _rand.Next(vMin, vMax + 1);

            // 3. Thực thi logic theo từng loại mã hành động (MaLoai)
            switch (type)
            {
                case "ATK":
                case "ATK_X2":
                    // Sát thương = (Tấn công gốc + Cường hóa hiện tại) * Hệ số
                    int multiplier = (type == "ATK_X2") ? 2 : 1;
                    int totalDmg = (monster.BaseAtk + monster.Strength) * multiplier;
                    hero.ApplyDamage(totalDmg);
                    return string.Format("{0} ({1} ST)", actionName, totalDmg);

                case "DEF":
                    monster.Armor += val;
                    return string.Format("{0} (+{1} Giáp)", actionName, val);

                case "STR_UP":
                    monster.Strength += val;
                    // Thêm hiệu ứng hiển thị Icon Cường hóa (turns=0 tương đương vĩnh viễn)
                    monster.AddEffect("STR", "⚔️", val, turns, actionName);
                    return string.Format("{0} (+{1} Công)", actionName, val);

                case "STRESS_UP":
                    // Tăng stress trực tiếp vào Database của người chơi
                    UpdateStat("StressHienTai", "StressToiDa", val, runID, strConn);
                    return actionName;

                case "EXHAUST_UP":
                    // Tăng kiệt sức trực tiếp vào Database của người chơi
                    UpdateStat("KietSucHienTai", "KietSucToiDa", val, runID, strConn);
                    return actionName;

                case "MASS_DEBUFF":
                    // Gây cả Stress và Kiệt sức đồng thời
                    UpdateStat("StressHienTai", "StressToiDa", val, runID, strConn);
                    UpdateStat("KietSucHienTai", "KietSucToiDa", val, runID, strConn);
                    return actionName;

                case "HOT":
                    // Hiệu ứng hồi máu mỗi lượt cho quái vật
                    monster.AddEffect("HOT", "💖", val, turns, actionName);
                    return actionName;

                case "STRESS_DOT":
                    // Hiệu ứng tăng Stress mỗi lượt cho người chơi
                    hero.AddEffect("S_DOT", "🧠", val, turns, actionName);
                    return actionName;

                case "ARMOR_DOT":
                    // Hiệu ứng tự cộng giáp mỗi lượt cho quái vật
                    monster.AddEffect("A_DOT", "🛡️", val, turns, actionName);
                    return actionName;

                case "POISON":
                    // Hiệu ứng nhiễm độc trừ máu mỗi lượt
                    hero.AddEffect("POISON", "🤢", val, turns, actionName);
                    return actionName;

                case "BLEED":
                    // Hiệu ứng chảy máu trừ máu mỗi lượt
                    hero.AddEffect("BLEED", "🩸", val, turns, actionName);
                    return actionName;

                case "SCOUT":
                    // Hút Mana của người chơi
                    UpdateStat("ManaHienTai", "ManaHienTai", -val, runID, strConn);
                    return actionName;

                case "HEAL":
                    // Hồi máu tức thì cho quái vật
                    monster.HP = Math.Min(monster.MaxHP, monster.HP + val);
                    return string.Format("{0} (+{1} HP)", actionName, val);

                default:
                    return actionName;
            }
        }

        // Cập nhật các chỉ số DB (Stress, Exhaustion, Mana) với logic Clamp (0 -> Max)
        private static void UpdateStat(string colHienTai, string colToiDa, int val, int rid, string connStr)
        {
            using (SqlConnection c = new SqlConnection(connStr))
            {
                c.Open();
                // SQL sử dụng CASE để đảm bảo giá trị không vượt quá Max và không nhỏ hơn 0
                string sql = string.Format(
                    "UPDATE LuotChoi SET {0} = (CASE " +
                    "WHEN {0} + {2} > {1} THEN {1} " +
                    "WHEN {0} + {2} < 0 THEN 0 " +
                    "ELSE {0} + {2} END) WHERE RunID = {3}",
                    colHienTai, colToiDa, val, rid);

                using (SqlCommand cmd = new SqlCommand(sql, c))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}