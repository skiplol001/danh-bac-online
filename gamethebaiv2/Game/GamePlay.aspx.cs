using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Text;
using System.Collections.Generic;

namespace gamethebaiv2.Game
{
    public partial class GamePlay : System.Web.UI.Page
    {
        string strConn = ConfigurationManager.ConnectionStrings["GameTheBai"].ConnectionString;

        // --- Cố định dữ liệu vào ViewState để chống lỗi Refresh ---
        public CombatEntity Hero
        {
            get { return (CombatEntity)ViewState["HeroData"]; }
            set { ViewState["HeroData"] = value; }
        }
        public CombatEntity Monster
        {
            get { return (CombatEntity)ViewState["MonsterData"]; }
            set { ViewState["MonsterData"] = value; }
        }
        public DataTable CurrentHand
        {
            get { return (DataTable)ViewState["HandData"]; }
            set { ViewState["HandData"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["CurrentRunID"] == null) Response.Redirect("../Default.aspx");

            if (!IsPostBack)
            {
                // Chỉ Init Battle khi chưa có dữ liệu (lần đầu vào trang)
                if (Hero == null || Monster == null)
                {
                    InitBattle();
                }
                else
                {
                    UpdateUI(); // Nạp lại UI từ ViewState nếu có sẵn
                }
            }
        }

        private void InitBattle()
        {
            Hero = new CombatEntity { Name = "Người Hồn Sư", MaxHP = 80, HP = 80 };
            Monster = new CombatEntity();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                // Reset Mana người chơi khi bắt đầu trận
                new SqlCommand("UPDATE LuotChoi SET ManaHienTai = 3 WHERE RunID = " + Session["CurrentRunID"], conn).ExecuteNonQuery();

                string mode = Request.QueryString["mode"] ?? "Normal";
                string sql = "SELECT TOP 1 * FROM QuaiVat WHERE ISNULL(LaBoss,0)=0 AND ISNULL(LaElite,0)=0 ORDER BY NEWID()";
                if (mode == "Boss") sql = "SELECT TOP 1 * FROM QuaiVat WHERE ISNULL(LaBoss,0)=1 ORDER BY NEWID()";
                else if (mode == "Elite") sql = "SELECT TOP 1 * FROM QuaiVat WHERE ISNULL(LaElite,0)=1 ORDER BY NEWID()";

                using (SqlDataReader dr = new SqlCommand(sql, conn).ExecuteReader())
                {
                    if (dr.Read())
                    {
                        ViewState["CurMID"] = dr["QuaiID"];
                        Monster.Name = dr["TenQuai"].ToString().ToUpper();
                        Monster.MaxHP = Monster.HP = Convert.ToInt32(dr["MauToiDa"]);
                        Monster.BaseAtk = Convert.ToInt32(dr["TanCongGoc"]);
                        Monster.IsElite = (dr["LaElite"] != DBNull.Value && Convert.ToBoolean(dr["LaElite"]));
                        Monster.ImgPath = dr["TenHinhAnh"].ToString();
                    }
                }
                PickNextMonsterCombo(conn);
                DrawNewHand(conn);
            }
            UpdateUI();
        }

        private void UpdateUI()
        {
            // Nạp hình ảnh (Sử dụng ResolveUrl để an toàn đường dẫn)
            imgHero.Src = ResolveUrl("~/Images/hero.png");
            imgEnemy.Src = ResolveUrl("~/Images/" + Monster.ImgPath);

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                using (SqlDataReader dr = new SqlCommand("SELECT * FROM LuotChoi WHERE RunID = " + Session["CurrentRunID"], conn).ExecuteReader())
                {
                    if (dr.Read())
                    {
                        lblPlayerName.Text = dr["TenNguoiChoi"].ToString().ToUpper();
                        lblMana.Text = dr["ManaHienTai"].ToString();
                        lblTopMana.Text = dr["ManaHienTai"].ToString();
                        lblGold.Text = dr["TienVang"].ToString();
                        lblPlayerStr.Text = (5 + Hero.Strength).ToString();
                        lblArmor.Text = Hero.Armor.ToString();

                        int hp = Convert.ToInt32(dr["MauHienTai"]);
                        int mhp = Convert.ToInt32(dr["MauToiDa"]);
                        pbPlayer.Style["width"] = (hp * 100 / mhp) + "%";
                        litPlayerHP.Text = hp + " / " + mhp;

                        litStress.Text = RenderVBar(Convert.ToInt32(dr["StressHienTai"]), "s-on");
                        litExhaust.Text = RenderVBar(Convert.ToInt32(dr["KietSucHienTai"]), "e-on");
                    }
                }
            }

            // Quái vật
            pbEnemy.Style["width"] = (Monster.HP * 100 / Math.Max(1, Monster.MaxHP)) + "%";
            litEnemyHP.Text = Monster.HP + " / " + Monster.MaxHP;
            lblEnemyName.InnerText = Monster.Name;
            lblMonsterIntent.Text = ViewState["NextIntent"]?.ToString();

            // Hiệu ứng (Buff/Debuff) - Lia chuột sẽ thấy description nhờ Tooltip
            rptHeroBuffs.DataSource = Hero.Buffs; rptHeroBuffs.DataBind();
            rptMonsterBuffs.DataSource = Monster.Buffs; rptMonsterBuffs.DataBind();

            // Bài trên tay
            rptHand.DataSource = CurrentHand; rptHand.DataBind();

            if (Hero.HP <= 0) HandleGameOver();
        }

        protected void btnEndTurn_Click(object sender, EventArgs e)
        {
            // 1. Tác động của hiệu ứng duy trì (DOT/HOT) trước khi quái đánh
            ProcessBuffTicks();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                Monster.Armor = 0; // Reset giáp quái khi bắt đầu lượt của nó

                // 2. LOGIC ELITE: HÀNH ĐỘNG 2 LẦN (Nếu là Elite)
                int loops = Monster.IsElite ? 2 : 1;
                for (int i = 0; i < loops; i++)
                {
                    DataTable dt = new DataTable();
                    string sql = "SELECT h.* FROM ChiTietCombo c JOIN HanhDongQuai h ON c.ActionID = h.ActionID WHERE c.ComboID = " + ViewState["NextID"] + " ORDER BY c.ThuTu";
                    new SqlDataAdapter(sql, conn).Fill(dt);
                    foreach (DataRow r in dt.Rows)
                    {
                        MonsterActionService.Execute(r, Monster, Hero, (int)Session["CurrentRunID"], strConn);
                    }
                }

                // 3. Kết thúc lượt: Đồng bộ máu Hero về DB và hồi Mana
                CombatEngine.SyncHP(Hero.HP, (int)Session["CurrentRunID"], strConn);
                new SqlCommand("UPDATE LuotChoi SET ManaHienTai = 3 WHERE RunID = " + Session["CurrentRunID"], conn).ExecuteNonQuery();

                PickNextMonsterCombo(conn); // Chuẩn bị ý định cho lượt sau
                DrawNewHand(conn); // Rút bài mới
                UpdateUI();
            }
        }

        private void ProcessBuffTicks()
        {
            // Người chơi hứng chịu debuff (Độc, chảy máu)
            foreach (var b in Hero.Buffs)
            {
                if (b.Key == "POISON" || b.Key == "BLEED") Hero.HP -= b.Value;
                if (b.Duration < 900) b.Duration--;
            }
            // Quái vật hồi phục (HOT, Armor DOT)
            foreach (var b in Monster.Buffs)
            {
                if (b.Key == "HOT") Monster.HP = Math.Min(Monster.MaxHP, Monster.HP + b.Value);
                if (b.Key == "A_DOT") Monster.Armor += b.Value;
                if (b.Duration < 900) b.Duration--;
            }
            // Xóa hiệu ứng hết hạn
            Hero.Buffs.RemoveAll(x => x.Duration <= 0);
            Monster.Buffs.RemoveAll(x => x.Duration <= 0);
        }

        private void PickNextMonsterCombo(SqlConnection conn)
        {
            using (SqlDataReader dr = new SqlCommand("SELECT TOP 1 ComboID, TenCombo FROM ComboQuai WHERE QuaiID = " + ViewState["CurMID"] + " ORDER BY NEWID()", conn).ExecuteReader())
            {
                if (dr.Read())
                {
                    ViewState["NextID"] = dr["ComboID"];
                    ViewState["NextIntent"] = "Ý ĐỊNH: " + dr["TenCombo"].ToString().ToUpper();
                }
            }
        }

        private void DrawNewHand(SqlConnection conn)
        {
            DataTable dt = new DataTable();
            string sql = "SELECT TOP 5 t.* FROM HanhTrangThe h JOIN ThuVienTheBai t ON h.TheID = t.TheID WHERE h.RunID = @rid ORDER BY NEWID()";
            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            da.SelectCommand.Parameters.AddWithValue("@rid", Session["CurrentRunID"]);
            da.Fill(dt);
            CurrentHand = dt;
        }

        private void HandleGameOver()
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                new SqlCommand("UPDATE LuotChoi SET MauHienTai=80, NutHienTaiID=1, StressHienTai=0, KietSucHienTai=0 WHERE RunID=" + Session["CurrentRunID"], conn).ExecuteNonQuery();
                new SqlCommand("DELETE FROM HanhTrangThe WHERE RunID=" + Session["CurrentRunID"], conn).ExecuteNonQuery();
            }
            ClientScript.RegisterStartupScript(this.GetType(), "GameOver", "alert('BẠN ĐÃ TỬ TRẬN!'); window.location='../Default.aspx';", true);
        }

        protected void rptHand_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "PlayCard")
            {
                int idx = Convert.ToInt32(e.CommandArgument);
                if (CurrentHand.Rows.Count <= idx) return;
                DataRow card = CurrentHand.Rows[idx];
                int cost = Convert.ToInt32(card["HaoTonMana"]);

                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    int mana = (int)new SqlCommand("SELECT ManaHienTai FROM LuotChoi WHERE RunID=" + Session["CurrentRunID"], conn).ExecuteScalar();
                    if (mana >= cost)
                    {
                        string code = card["MaThe"].ToString();
                        // Thực thi hiệu ứng thẻ bài
                        if (code.Contains("ATK")) Monster.ApplyDamage(5 + Hero.Strength + Convert.ToInt32(card["CongSatThuong"]));
                        else if (code.Contains("DEF")) Hero.Armor += Convert.ToInt32(card["GiaTriGiap"]);

                        new SqlCommand("UPDATE LuotChoi SET ManaHienTai -= " + cost + " WHERE RunID=" + Session["CurrentRunID"], conn).ExecuteNonQuery();
                        CurrentHand.Rows.RemoveAt(idx);

                        if (Monster.HP <= 0) Response.Redirect("Play.aspx"); // Thắng quái
                        UpdateUI();
                    }
                }
            }
        }

        private string RenderVBar(int val, string css)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= 10; i++) sb.AppendFormat("<div class='v-seg {0}'></div>", i <= val ? css : "");
            return sb.ToString();
        }

        protected string GetDynamicDesc(object m, object c, object g, object d)
        {
            if (m.ToString().Contains("ATK")) return "Gây " + (5 + Hero.Strength + Convert.ToInt32(c)) + " ST.";
            return d.ToString();
        }
    }
}