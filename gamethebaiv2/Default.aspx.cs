using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System;

namespace gamethebaiv2
{
    public partial class Default : System.Web.UI.Page
    {
        string strConn = ConfigurationManager.ConnectionStrings["GameTheBai"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CheckActiveRun();
            }
        }

        // KIỂM TRA FILE SAVE
        private void CheckActiveRun()
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                // Tìm lượt chơi gần nhất còn sống
                string sql = "SELECT TOP 1 RunID, TenNguoiChoi FROM LuotChoi WHERE TrangThai = 'Active' AND MauHienTai > 0 ORDER BY RunID DESC";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    btnContinue.Visible = true;
                    lblPlayerName.Text = dr["TenNguoiChoi"].ToString();
                    Session["CurrentRunID"] = dr["RunID"];
                }
                dr.Close();
            }
        }

        protected void btnContinue_Click(object sender, EventArgs e)
        {
            // Nếu Session mất, bốc lại từ DB lần nữa cho chắc
            if (Session["CurrentRunID"] != null)
                Response.Redirect("Game/Play.aspx");
            else
                CheckActiveRun();
        }

        protected void btnNewGame_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // 1. Vô hiệu hóa các lượt chơi cũ
                    new SqlCommand("UPDATE LuotChoi SET TrangThai = 'Inactive' WHERE TrangThai = 'Active'", conn, trans).ExecuteNonQuery();

                    // 2. Tạo lượt chơi mới (Chỉ số mặc định chuẩn Roguelike)
                    string sqlNewRun = @"INSERT INTO LuotChoi 
                        (TenNguoiChoi, MauHienTai, MauToiDa, ManaHienTai, TienVang, AtkHienTai, NutHienTaiID, TrangThai, StressHienTai, StressToiDa, KietSucHienTai, KietSucToiDa) 
                        OUTPUT INSERTED.RunID
                        VALUES (N'Người Hồn Sư', 80, 80, 3, 50, 5, 1, 'Active', 0, 10, 0, 10)";

                    int newRunID = (int)new SqlCommand(sqlNewRun, conn, trans).ExecuteScalar();
                    Session["CurrentRunID"] = newRunID;

                    // 3. TẶNG 5 LÁ BÀI KHỞI ĐẦU (Giả sử ID 1: Tấn công, ID 2: Phòng thủ)
                    // Bạn có thể thay đổi ID thẻ bài cho đúng với bảng ThuVienTheBai của bạn
                    int[] starterCards = { 1, 1, 1, 2, 2 }; // 3 lá công, 2 lá thủ
                    foreach (int cardID in starterCards)
                    {
                        SqlCommand cmdCard = new SqlCommand("INSERT INTO HanhTrangThe (RunID, TheID) VALUES (@rid, @tid)", conn, trans);
                        cmdCard.Parameters.AddWithValue("@rid", newRunID);
                        cmdCard.Parameters.AddWithValue("@tid", cardID);
                        cmdCard.ExecuteNonQuery();
                    }

                    trans.Commit();
                    Response.Redirect("Game/Play.aspx");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Response.Write("<script>alert('Lỗi khởi tạo game: " + ex.Message + "');</script>");
                }
            }
        }

        protected void btnExit_Click(object sender, EventArgs e)
        {
            // Logic thoát game (thường là đóng tab hoặc về trang giới thiệu)
            Response.Write("<script>window.close();</script>");
        }
    }
}