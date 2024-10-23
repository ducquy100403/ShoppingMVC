namespace Shopping_Tutortial.Models
{
    public class Paginate
    {
        public int TotalItems { get; private set; } //Tôngt số item
        public int PageSize { get; private set; }// tống số item/trang
        public int CurrentPage { get; private set; }// trang hiện tại
        public int TotalPages { get; private set; }// tổng trang
        public int StartPage { get; private set; }// trang bắt đầu
        public int EndPage { get; private set; }// trang kết thúc

        public Paginate()
        {

        }

        public Paginate(int totalItems,int page , int pageSize = 10) // 10item /trang
        {
            //làm tròn tổng item/10 iteam trên trang
            int totalPages = (int)Math.Ceiling((decimal)totalItems/(decimal)pageSize);

            int currentPage = page;// trang hien tai = 1

            int startPage = currentPage - 5; //trang bắt đầu trừ 5 button

            int endPage = currentPage + 4;//trang cuối sẽ cộng thêm 4 button 

            if (startPage <= 0) 
            {
                //nếu số trang bắt đầu nhỏ hơn hoặc bằng 0 thì số trang cuối sẽ bằng
                endPage = endPage-(startPage-1);
                startPage = 1;
            }
            if (endPage > totalPages)// nếu số page cuối > số tổng trang
            {
                endPage = totalPages;// số trang cuối = số tổng trang
                if(endPage > 10)// nếu số trang cuối > 10
                {
                    startPage = endPage - 9;//trang bắt đầu = trang cuối -9
                }    
            }
            TotalItems = totalItems;
            TotalPages = totalPages;
            CurrentPage = currentPage;
            PageSize = pageSize;
            StartPage = startPage;
            EndPage = endPage;
        }    
    }
}
