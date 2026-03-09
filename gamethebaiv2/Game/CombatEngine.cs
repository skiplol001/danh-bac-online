using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace gamethebaiv2.Game
{
    // 1. LỚP QUẢN LÝ HIỆU ỨNG (BUFF/DEBUFF)
    [Serializable]
    public class ActiveStatus
    {
        public string Key { get; set; }        // Mã hiệu ứng (POISON, STR, ...)
        public string Icon { get; set; }       // Emoji hiển thị (🤢, ⚔️, ...)
        public int Value { get; set; }         // Giá trị hiệu ứng
        public int Duration { get; set; }      // Số lượt (>=900 là vĩnh viễn)
        public string Description { get; set; } // Mô tả hiện khi di chuột vào
        public string BaseName { get; set; }    // Tên gốc (Nhiễm độc, Cường hóa...)
    }

    // 2. LỚP THỰC THỂ CHIẾN ĐẤU (HERO & MONSTER)
    [Serializable]
    public class CombatEntity
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Armor { get; set; }
        public int Strength { get; set; }
        public int BaseAtk { get; set; }
        public bool IsElite { get; set; }      // Quái tinh anh (hành động 2 lần)
        public string ImgPath { get; set; }     // Đường dẫn ảnh
        public List<ActiveStatus> Buffs { get; set; }

        public CombatEntity()
        {
            Buffs = new List<ActiveStatus>();
            Strength = 0;
            Armor = 0;
            IsElite = false;
            ImgPath = "";
        }

        // Logic nhận sát thương có trừ giáp
        public void ApplyDamage(int dmg)
        {
            int realDmg = dmg - Armor;
            if (realDmg < 0) realDmg = 0;

            // Giáp bị trừ bởi tổng sát thương
            Armor -= dmg;
            if (Armor < 0) Armor = 0;

            HP -= realDmg;
            if (HP < 0) HP = 0;
        }

        // Logic thêm hiệu ứng
        public void AddEffect(string key, string icon, int val, int durationFromDB, string baseName)
        {
            // Quy ước: Soluot = 0 trong DB nghĩa là vĩnh viễn (999)
            int finalDuration = (durationFromDB <= 0) ? 999 : durationFromDB;

            ActiveStatus existing = Buffs.Find(x => x.Key == key);
            if (existing != null)
            {
                if (key == "STR" || key == "STR_BUFF") existing.Value += val;
                else existing.Value = val;
                existing.Duration = Math.Max(existing.Duration, finalDuration);
            }
            else
            {
                Buffs.Add(new ActiveStatus
                {
                    Key = key,
                    Icon = icon,
                    Value = val,
                    Duration = finalDuration,
                    BaseName = baseName
                });
            }
            RefreshDescription(key);
        }

        public void RefreshDescription(string key)
        {
            ActiveStatus effect = Buffs.Find(x => x.Key == key);
            if (effect == null) return;
            string timeStr = effect.Duration >= 900 ? "Vĩnh viễn" : effect.Duration + " lượt";

            if (key == "STR")
                effect.Description = string.Format("{0}: +{1} Công ({2})", effect.BaseName, effect.Value, timeStr);
            else
                effect.Description = string.Format("{0}: {1}/lượt ({2})", effect.BaseName, effect.Value, timeStr);
        }
    }

    // 3. LỚP CƠ CHẾ CHIẾN ĐẤU (ĐÂY LÀ PHẦN BẠN ĐANG THIẾU)
    public static class CombatEngine
    {
        // Đồng bộ máu về Database
        public static void SyncHP(int hp, int rid, string connStr)
        {
            if (string.IsNullOrEmpty(connStr)) return;

            using (SqlConnection c = new SqlConnection(connStr))
            {
                c.Open();
                string sql = "UPDATE LuotChoi SET MauHienTai = @hp WHERE RunID = @rid";
                using (SqlCommand cmd = new SqlCommand(sql, c))
                {
                    cmd.Parameters.AddWithValue("@hp", hp);
                    cmd.Parameters.AddWithValue("@rid", rid);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}