using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace gamethebaiv2.Game
{
    public partial class Shop : System.Web.UI.Page
    {
        string strConn = ConfigurationManager.ConnectionStrings["GameTheBai"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["CurrentRunID"] == null) Response.Redirect("../Default.aspx");

            if (!IsPostBack)
            {
                LoadShopCards();
            }
            UpdateGoldDisplay();
        }

        private void UpdateGoldDisplay()
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                lblGold.Text = new SqlCommand("SELECT TienVang FROM LuotChoi WHERE RunID=" + Session["CurrentRunID"], conn).ExecuteScalar().ToString();
            }
        }

        private void LoadShopCards()
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                // Lấy 3 lá bài ngẫu nhiên từ thư viện thẻ bài
                string sql = "SELECT TOP 3 * FROM ThuVienTheBai ORDER BY NEWID()";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptCards.DataSource = dt;
                rptCards.DataBind();
            }
        }

        // XỬ LÝ MUA BÀI (Giá cố định 75 Vàng)
        protected void rptCards_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "BuyCard")
            {
                int theId = Convert.ToInt32(e.CommandArgument);
                int price = 75;

                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    int myGold = (int)new SqlCommand("SELECT TienVang FROM LuotChoi WHERE RunID=" + Session["CurrentRunID"], conn).ExecuteScalar();

                    if (myGold >= price)
                    {
                        SqlTransaction trans = conn.BeginTransaction();
                        try
                        {
                            // 1. Trừ tiền
                            new SqlCommand("UPDATE LuotChoi SET TienVang -= " + price + " WHERE RunID=" + Session["CurrentRunID"], conn, trans).ExecuteNonQuery();
                            // 2. Thêm vào hành trang
                            SqlCommand cmdAdd = new SqlCommand("INSERT INTO HanhTrangThe (RunID, TheID) VALUES (@rid, @tid)", conn, trans);
                            cmdAdd.Parameters.AddWithValue("@rid", Session["CurrentRunID"]);
                            cmdAdd.Parameters.AddWithValue("@tid", theId);
                            cmdAdd.ExecuteNonQuery();

                            trans.Commit();
                            Response.Write("<script>alert('Mua thẻ bài thành công!');</script>");
                        }
                        catch { trans.Rollback(); }
                    }
                    else Response.Write("<script>alert('Không đủ vàng!');</script>");
                }
                UpdateGoldDisplay();
            }
        }

        // XỬ LÝ HỒI MÁU (Giá 50 Vàng)
        protected void btnHeal_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                int myGold = (int)new SqlCommand("SELECT TienVang FROM LuotChoi WHERE RunID=" + Session["CurrentRunID"], conn).ExecuteScalar();

                if (myGold >= 50)
                {
                    // Hồi 25 máu nhưng không vượt quá máu tối đa
                    string sql = @"UPDATE LuotChoi 
                                   SET TienVang -= 50, 
                                       MauHienTai = CASE WHEN MauHienTai + 25 > MauToiDa THEN MauToiDa ELSE MauHienTai + 25 END 
                                   WHERE RunID = " + Session["CurrentRunID"];
                    new SqlCommand(sql, conn).ExecuteNonQuery();
                    Response.Write("<script>alert('Đã hồi phục 25 Máu!');</script>");
                }
                else Response.Write("<script>alert('Không đủ vàng!');</script>");
            }
            UpdateGoldDisplay();
        }
    }
}