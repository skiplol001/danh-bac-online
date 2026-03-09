using System;
using System.Data;
using System.Data.SqlClient; // FIX LỖI CS0246
using System.Configuration;  // FIX LỖI CS0103
using System.Web.UI.WebControls;
using System.Web;

namespace gamethebaiv2.Game
{
    public partial class Play : System.Web.UI.Page
    {
        string strConn = ConfigurationManager.ConnectionStrings["GameTheBai"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["CurrentRunID"] == null) Response.Redirect("../Default.aspx");

            // Khởi tạo chuỗi hành trình nếu là lượt chơi mới hoàn toàn
            if (Session["PathHistory"] == null) Session["PathHistory"] = "";

            if (!IsPostBack)
            {
                if (Request.QueryString["node"] != null)
                    HandleNodeSelection(Convert.ToInt32(Request.QueryString["node"]));
                LoadMapData();
            }
            LoadHeroStats();
            // Gửi dữ liệu hành trình xuống cho JavaScript vẽ đường xanh
            hfHistory.Value = Session["PathHistory"].ToString();
        }

        private void HandleNodeSelection(int newNodeId)
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                int curId = GetCurrentNodeID(conn);

                SqlCommand check = new SqlCommand("SELECT COUNT(*) FROM DuongNoiBanDo WHERE NutTruocID=@c AND NutSauID=@n", conn);
                check.Parameters.AddWithValue("@c", curId);
                check.Parameters.AddWithValue("@n", newNodeId);

                if ((int)check.ExecuteScalar() > 0)
                {
                    // LƯU VẾT HÀNH TRÌNH: Thêm vị trí cũ vào chuỗi lịch sử
                    string history = Session["PathHistory"].ToString();
                    if (!history.Contains(curId.ToString() + ","))
                    {
                        Session["PathHistory"] = history + curId + ",";
                    }

                    new SqlCommand("UPDATE LuotChoi SET NutHienTaiID=" + newNodeId + " WHERE RunID=" + Session["CurrentRunID"], conn).ExecuteNonQuery();

                    string type = new SqlCommand("SELECT LoaiNut FROM NutBanDo WHERE NutID=" + newNodeId, conn).ExecuteScalar().ToString();

                    // PHÂN LOẠI ĐIỀU HƯỚNG
                    if (type.Contains("Combat")) Response.Redirect("GamePlay.aspx?mode=" + (type == "CombatHigh" ? "Elite" : "Normal"));
                    else if (type == "Shop") Response.Redirect("Shop.aspx");
                    else if (type == "Boss") Response.Redirect("GamePlay.aspx?mode=Boss");
                    else if (type == "Event") ShowRandomEvent();
                }
                else Response.Redirect("Play.aspx");
            }
        }

        public string GetNodeStatus(object nutId)
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                int curId = GetCurrentNodeID(conn);
                int nid = Convert.ToInt32(nutId);
                string history = Session["PathHistory"].ToString();

                if (nid == curId) return "current";

                // Nút đã đi qua trong lịch sử: Phải sáng màu xanh
                if (history.Contains(nid.ToString() + ",")) return "visited";

                // Nút có thể đi từ vị trí hiện tại
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM DuongNoiBanDo WHERE NutTruocID=@c AND NutSauID=@n", conn);
                cmd.Parameters.AddWithValue("@c", curId); cmd.Parameters.AddWithValue("@n", nid);
                if ((int)cmd.ExecuteScalar() > 0) return "reachable";

                // Nút bị bỏ qua hoặc bị khóa: Nhuộm xám
                return "locked";
            }
        }

        // --- HỆ THỐNG 3 SỰ KIỆN MẠO HIỂM (GIỮ NGUYÊN 100%) ---
        private void ShowRandomEvent()
        {
            pnlEvent.Visible = true;
            Random rand = new Random();
            int evId = rand.Next(1, 4);
            ViewState["EvType"] = evId;
            if (evId == 1)
            {
                eventTitle.InnerText = "DI VẬT MẠO HIỂM GIẢ";
                eventDesc.InnerText = "Xác một chiến binh nằm đó. Những trang bị này có thể giúp bạn đi xa hơn...";
                btnChoice1.Text = "Bao tay da (+2 Máu tối đa)"; btnChoice2.Text = "Nhẫn thông tuệ (+2 Stress tối đa)"; btnChoice3.Text = "Suối nguồn sinh lực (+2 Kiệt sức tối đa)";
            }
            else if (evId == 2)
            {
                eventTitle.InnerText = "BÁC SĨ DỊCH BỆNH";
                eventDesc.InnerText = "Hắn chìa ra những lọ thuốc bốc khói lấp lánh màu nhiệm...";
                btnChoice1.Text = "Bình đỏ (+1 Máu tối đa)"; btnChoice2.Text = "Bình tím (+1 Tấn công cơ sở)"; btnChoice3.Text = "Cướp vàng của hắn (+25 Vàng)";
            }
            else
            {
                eventTitle.InnerText = "HÀNH LANG TỐI TĂM";
                eventDesc.InnerText = "Bóng tối bao phủ, một giọng nói thì thầm đánh đổi sự minh mẫn lấy sức mạnh...";
                btnChoice1.Text = "Mạo hiểm Stress (60% tăng Max / 40% tăng Hiện tại)"; btnChoice2.Text = "Dùng dược phẩm rơi (60% tăng Max / 40% tăng Hiện tại)"; btnChoice3.Text = "Đi tiếp lặng lẽ";
            }
        }

        protected void Choice_Click(object sender, EventArgs e)
        {
            int ch = Convert.ToInt32(((Button)sender).CommandArgument);
            int ev = (int)ViewState["EvType"];
            string sql = ""; Random r = new Random(); int rate = r.Next(1, 101);
            if (ev == 1)
            {
                if (ch == 1) sql = "UPDATE LuotChoi SET MauToiDa += 2, MauHienTai += 2";
                else if (ch == 2) sql = "UPDATE LuotChoi SET StressToiDa += 2";
                else if (ch == 3) sql = "UPDATE LuotChoi SET KietSucToiDa += 2";
            }
            else if (ev == 2)
            {
                if (ch == 1) sql = "UPDATE LuotChoi SET MauToiDa += 1, MauHienTai += 1";
                else if (ch == 2) sql = "UPDATE LuotChoi SET AtkHienTai += 1";
                else if (ch == 3) sql = "UPDATE LuotChoi SET TienVang += 25";
            }
            else if (ev == 3)
            {
                if (ch == 1)
                {
                    if (rate <= 60) sql = "UPDATE LuotChoi SET StressToiDa += 4";
                    else sql = "UPDATE LuotChoi SET StressHienTai = (CASE WHEN StressHienTai+4 > StressToiDa THEN StressToiDa ELSE StressHienTai+4 END)";
                }
                else if (ch == 2)
                {
                    if (rate <= 60) sql = "UPDATE LuotChoi SET KietSucToiDa += 4";
                    else sql = "UPDATE LuotChoi SET KietSucHienTai = (CASE WHEN KietSucHienTai+4 > KietSucToiDa THEN KietSucToiDa ELSE KietSucHienTai+4 END)";
                }
            }
            if (!string.IsNullOrEmpty(sql))
            {
                using (SqlConnection c = new SqlConnection(strConn)) { c.Open(); new SqlCommand(sql + " WHERE RunID=" + Session["CurrentRunID"], c).ExecuteNonQuery(); }
            }
            pnlEvent.Visible = false; LoadHeroStats();
        }

        private void LoadHeroStats()
        {
            try
            {
                // 1. Kiểm tra tuyệt đối HttpContext và Session để dập lỗi Line 164
                if (HttpContext.Current == null || HttpContext.Current.Session == null) return;
                if (Session["CurrentRunID"] == null) return;

                // 2. Kiểm tra Connection String (Đề phòng Web.config sai tên)
                var connSettings = ConfigurationManager.ConnectionStrings["GameTheBai"];
                if (connSettings == null || string.IsNullOrEmpty(connSettings.ConnectionString)) return;

                using (SqlConnection conn = new SqlConnection(connSettings.ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM LuotChoi WHERE RunID = @rid";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@rid", Session["CurrentRunID"]);
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr != null && dr.Read())
                            {
                                // 3. Lấy dữ liệu an toàn (Xử lý DBNull cho các cột mới thêm)
                                int hp = (dr["MauHienTai"] != DBNull.Value) ? Convert.ToInt32(dr["MauHienTai"]) : 0;
                                int mhp = (dr["MauToiDa"] != DBNull.Value) ? Convert.ToInt32(dr["MauToiDa"]) : 1;
                                int str = (dr["StressHienTai"] != DBNull.Value) ? Convert.ToInt32(dr["StressHienTai"]) : 0;
                                int mstr = (dr["StressToiDa"] != DBNull.Value) ? Convert.ToInt32(dr["StressToiDa"]) : 1;
                                int kiet = (dr["KietSucHienTai"] != DBNull.Value) ? Convert.ToInt32(dr["KietSucHienTai"]) : 0;
                                int mkiet = (dr["KietSucToiDa"] != DBNull.Value) ? Convert.ToInt32(dr["KietSucToiDa"]) : 1;

                                // 4. KIỂM TRA NULL CHO CONTROL (Dập lỗi Designer mất kết nối)
                                // Chỉ gán khi Control thực sự tồn tại trong bộ nhớ
                                if (lblHP != null) lblHP.Text = hp + "/" + mhp;
                                if (pbHP != null) pbHP.Style["width"] = (hp * 100 / Math.Max(1, mhp)) + "%";

                                if (lblStress != null) lblStress.Text = str + "/" + mstr;
                                if (pbStress != null) pbStress.Style["width"] = (str * 100 / Math.Max(1, mstr)) + "%";

                                if (lblExhaust != null) lblExhaust.Text = kiet + "/" + mkiet;
                                if (pbExhaust != null) pbExhaust.Style["width"] = (kiet * 100 / Math.Max(1, mkiet)) + "%";

                                if (lblGold != null) lblGold.Text = (dr["TienVang"] != DBNull.Value) ? dr["TienVang"].ToString() : "0";
                                if (lblATK != null) lblATK.Text = (dr["AtkHienTai"] != DBNull.Value) ? dr["AtkHienTai"].ToString() : "0";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log ra cửa sổ Output của Visual Studio để debug, không làm sập trang web
                System.Diagnostics.Debug.WriteLine("Lỗi LoadHeroStats: " + ex.Message);
            }
        }
        private void LoadMapData()
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                DataTable dt = new DataTable();
                new SqlDataAdapter("SELECT DISTINCT Tang FROM NutBanDo ORDER BY Tang ASC", conn).Fill(dt);
                rptFloors.DataSource = dt; rptFloors.DataBind();
                DataTable dtPaths = new DataTable();
                new SqlDataAdapter("SELECT NutTruocID, NutSauID FROM DuongNoiBanDo", conn).Fill(dtPaths);
                rptPaths.DataSource = dtPaths; rptPaths.DataBind();
            }
        }

        private int GetCurrentNodeID(SqlConnection conn)
        {
            return (int)new SqlCommand("SELECT NutHienTaiID FROM LuotChoi WHERE RunID=" + Session["CurrentRunID"], conn).ExecuteScalar();
        }

        public DataTable GetNodes(object tang)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                new SqlDataAdapter("SELECT * FROM NutBanDo WHERE Tang=" + tang, conn).Fill(dt);
            }
            return dt;
        }
    }
}