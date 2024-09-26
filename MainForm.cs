using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Maze
{
    public partial class MainForm : Form
    {

        private class MyMaze
        {

            private int dimensionX, dimensionY; // kích thước mê cung
            public int gridDimensionX, gridDimensionY; // kích thước đầu ra lưới
            public char[,] mazeGrid; // đầu ra lưới
            private Cell[,] cells; // mảng 2 chiều
            private Random random = new Random(); // random

            public MyMaze(int xDimension, int yDimension)
            {
                dimensionX = xDimension;              // kích thước mê cung
                dimensionY = yDimension;
                gridDimensionX = xDimension * 2 + 1;  // kích thước đầu ra lưới
                gridDimensionY = yDimension * 2 + 1;
                mazeGrid = new char[gridDimensionX, gridDimensionY];
                Init();
                GenerateMaze();
            }

            private void Init()
            {
                // tạo ô vuông
                cells = new Cell[dimensionX, dimensionY];
                for (int x = 0; x < dimensionX; x++)
                    for (int y = 0; y < dimensionY; y++)
                        cells[x, y] = new Cell(x, y, false);
            }

            // biểu diễn 1 ô
            public class Cell
            {
                public int x, y; // tọa độ
                // các ô mà ô này kết nối tới
                ArrayList neighbors = new ArrayList();
                // ô không thể đi qua
                public bool wall = true;
                // Nếu đúng, ô này chưa được sử dụng trong quá trình tạo
                public bool open = true;
                // Tạo ô tại vị trí x, y
                public Cell(int x, int y)
                {
                    this.x = x;
                    this.y = y;
                    wall = true;
                }
                // Tạo đối tượng Cell tại vị trí x, y và xác định liệu nó có phải là tường hay không
                public Cell(int x, int y, bool isWall)
                {
                    this.x = x;
                    this.y = y;
                    wall = isWall;
                }
                // ạo đối tượng Cell tại vị trí x, y và xác định liệu nó có phải là tường hay không
                public void AddNeighbor(Cell other)
                {
                    if (!this.neighbors.Contains(other))
                        // tránh các bản sao
                        this.neighbors.Add(other);
                    if (!other.neighbors.Contains(this))
                        // tránh các bản sao
                        other.neighbors.Add(this);
                }
                // sử dụng trong hàm updateGrid()
                public bool IsCellBelowNeighbor()
                {
                    return this.neighbors.Contains(new Cell(this.x, this.y + 1));
                }
                // sử dụng trong hàm updateGrid()
                public bool IsCellRightNeighbor()
                {
                    return this.neighbors.Contains(new Cell(this.x + 1, this.y));
                }

                // các Cell tương đương hữu ích
                public override bool Equals(Object other)
                {
                    // nếu đối tượng orther không thuộc Cell thì trả false
                    if (other.GetType() != typeof(Cell))
                        return false;
                    Cell otherCell = (Cell)other;
                    return (x == otherCell.x) && (y == otherCell.y);
                }

                // nên được ghi đè với equals
                public override int GetHashCode()
                {
                    return x + y * 256;
                }

            }
            // Tạo từ góc trên bên trái (Trong tính toán, thường thì y tăng khi đi xuống)
            private void GenerateMaze()
            {
                GenerateMaze(0, 0);
            }
            // Tạo mê cung từ tọa độ x, y
            private void GenerateMaze(int x, int y)
            {
                GenerateMaze(GetCell(x, y)); // Tạo từ ô
            }
            private void GenerateMaze(Cell startAt)
            {
                // Không tạo ra từ ô không có
                if (startAt == null) return;
                startAt.open = false; // Đánh dấu ô đã đóng để tạo mê cung
                var cellsList = new ArrayList { startAt };

                while (cellsList.Count > 0)
                {
                    Cell cell;
                    // Đoạn mã này giúp giảm số lượng những hành lang uốn lượn dài nhưng 
                    //vẫn giữ lại một số nhánh ngắn dễ phát hiện
                    // Dẫn đến việc tạo ra mê cung dễ dàng hơn
                    if (random.Next(10) == 0)
                    {
                        cell = (Cell)cellsList[random.Next(cellsList.Count)];
                        cellsList.RemoveAt(random.Next(cellsList.Count));
                    }

                    else
                    {
                        cell = (Cell)cellsList[cellsList.Count - 1];
                        cellsList.RemoveAt(cellsList.Count - 1);
                    }
                    // Đối với bộ sưu tập
                    ArrayList neighbors = new ArrayList();
                    // Các ô có thể là hàng xóm tiềm năng
                    Cell[] potentialNeighbors = new Cell[]{
                        GetCell(cell.x + 1, cell.y),
                        GetCell(cell.x, cell.y + 1),
                        GetCell(cell.x - 1, cell.y),
                        GetCell(cell.x, cell.y - 1)
                    };
                    foreach (Cell other in potentialNeighbors)
                    {
                        // Bỏ qua nếu ngoài phạm vi, là tường hoặc không mở
                        if (other == null || other.wall || !other.open)
                            continue;
                        neighbors.Add(other);
                    }
                    if (neighbors.Count == 0) continue;
                    // Chọn một ô ngẫu nhiên
                    Cell selected = (Cell)neighbors[random.Next(neighbors.Count)];
                    // Đánh dấu ô đã đóng để tạo mê cung
                    selected.open = false; // Thêm vào danh sách hàng xóm
                    cell.AddNeighbor(selected);
                    cellsList.Add(cell);
                    cellsList.Add(selected);
                }
                UpdateGrid();
            }
            // Hàm để lấy ô Cell tại vị trí x, y; trả về null ngoài phạm vi


            public Cell GetCell(int x, int y)
            {
                try
                {
                    return cells[x, y];
                }
                catch (IndexOutOfRangeException)
                { // Bắt ngoại lệ khi vượt ra ngoài giới hạn
                    return null;
                }
            }
            // Vẽ mê cung
            public void UpdateGrid()
            {
                char backChar = ' ', wallChar = 'X', cellChar = ' ';
                // Điền nền
                for (int x = 0; x < gridDimensionX; x++)
                    for (int y = 0; y < gridDimensionY; y++)
                        mazeGrid[x, y] = backChar;
                // Xây dựng các tường
                for (int x = 0; x < gridDimensionX; x++)
                    for (int y = 0; y < gridDimensionY; y++)
                        if (x % 2 == 0 || y % 2 == 0)
                            mazeGrid[x, y] = wallChar;
                // Tạo biểu diễn ý nghĩa
                for (int x = 0; x < dimensionX; x++)
                    for (int y = 0; y < dimensionY; y++)
                    {
                        Cell current = GetCell(x, y);
                        int gridX = x * 2 + 1, gridY = y * 2 + 1;
                        mazeGrid[gridX, gridY] = cellChar;
                        if (current.IsCellBelowNeighbor())
                            mazeGrid[gridX, gridY + 1] = cellChar;
                        if (current.IsCellRightNeighbor())
                            mazeGrid[gridX + 1, gridY] = cellChar;
                    }
            }
        }
        // Kết thúc lớp MyMaze

        //++++++++++++++++++++++++++++++++++++++

        /// Lớp trợ giúp đại diện cho ô trên lưới
        private class Cell
        {
            public int row;     // Số thứ tự hàng của ô (hàng 0 là hàng trên cùng)
            public int col;     // Số thứ tự cột của ô (Cột 0 là cột bên trái)
            public double g;    // Giá trị của hàm g trong các thuật toán A* và Greedy
            public double h;    // Giá trị của hàm h trong các thuật toán A* và Greedy
            public double f;    // Giá trị của hàm f trong các thuật toán A* và Greedy
            public double dist; // Khoảng cách từ ô đến vị trí ban đầu của robot
            public int level;   // Dùng như tham số thứ hai để sắp xếp danh sách 'openset' hoặc 'graph'
            public Cell prev;   // Mỗi trạng thái tương ứng với một ô
                                // và mỗi trạng thái có một ô tiền nhiệm được lưu trong biến này
            public Cell(int row, int col)
            {
                this.row = row;
                this.col = col;
            }

        } // Kết thúc lớp Cell

        /*
         **********************************************************
         *          Các hằng số của lớp MazePanel
         **********************************************************
         */
        const int INFINITY = Int32.MaxValue; // Biểu diễn của vô cùng
        const int EMPTY = 0;      // Ô trống
        const int OBST = 1;       // Ô có chướng ngại vật
        const int ROBOT = 2;      // Vị trí của robot
        const int TARGET = 3;     // Vị trí của mục tiêu
        const int FRONTIER = 4;   // Các ô tạo thành biên giới (OPEN SET)
        const int CLOSED = 5;     // Các ô tạo thành tập đóng (CLOSED SET)
        const int ROUTE = 6;      // Các ô tạo thành đường đi từ robot đến mục tiêu
        /*
         **********************************************************
         *          Các biến của lớp MazePanel
         **********************************************************
         */
        int rows;        // Số hàng của lưới
        int columns;     // Số cột của lưới
        int squareSize;  // Kích thước của ô trong đơn vị pixel
        int arrowSize;   // Kích thước của mũi tên chỉ đến ô tiền nhiệm

        List<Cell> openSet = new List<Cell>(); // Tập OPEN SET
        List<Cell> closedSet = new List<Cell>(); // Tập CLOSED SET
        List<Cell> graph = new List<Cell>(); // Tập các đỉnh của đồ thị
                                             // được khám phá bởi thuật toán Dijkstra

        Cell robotStart;  // Vị trí ban đầu của robot
        Cell targetPos;   // Vị trí của mục tiêu

        int[,] grid;      // Lưới các ô
        bool realTime;    // Hiển thị giải pháp ngay lập tức
        bool found;       // Cờ chỉ ra rằng đã tìm thấy mục tiêu
        bool searching;   // Cờ chỉ ra rằng quá trình tìm kiếm đang diễn ra
        bool endOfSearch; // Cờ chỉ ra rằng quá trình tìm kiếm đã kết thúc
        bool animation;   // Cờ chỉ ra rằng hoạt hình đang chạy
        int delay;        // Độ trễ thời gian của hoạt hình (đơn vị là mili giây)
        int expanded;     // Số lượng nút đã được mở rộng
        int level;        // Theo dõi việc tạo ra các kế tự của các ô kế nhiệm

        bool mouse_down = false;
        int cur_row, cur_col, cur_val;

    
        /// Constructor (Hàm tạo)
        public MainForm()
        {
            InitializeComponent();
            dfs.Checked = true; // Thiết lập mặc định chọn thuật toán DFS
            challengeTimer = new Timer();
            challengeTimer.Interval = 1000; // Cài đặt khoảng thời gian là 1 giây
            challengeTimer.Tick += new EventHandler(ChallengeTimer_Tick); // Gán sự kiện Tick
                                                                          // Thiết lập giá trị mặc định cho elapsedTime
            elapsedTime = 0;
            InitializeGrid(false); // Khởi tạo lưới mê cung với tham số false
        } // Kết thúc hàm tạo MainForm



        /// Xử lý sự kiện click chuột để thêm hoặc xóa các chướng ngại vật trên lưới
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            mouse_down = true; // Đánh dấu chuột đã được nhấn xuống
            int row = (e.Y - 10) / squareSize; // Xác định hàng dựa trên vị trí Y của chuột
            int col = (e.X - 10) / squareSize; // Xác định cột dựa trên vị trí X của chuột
            if (row >= 0 && row < rows && col >= 0 && col < columns)
            {
                // Chỉ cho phép thao tác nếu là thời gian thực hoặc chưa tìm thấy và không đang tìm kiếm
                if (realTime ? true : !found && !searching)
                {
                    if (realTime)
                        FillGrid(); // Đổ lại lưới nếu là thời gian thực
                    cur_row = row;
                    cur_col = col;
                    cur_val = grid[row, col];
                    // Thêm hoặc xóa chướng ngại vật tại ô được click
                    if (cur_val == EMPTY)
                        grid[row, col] = OBST; // Thêm chướng ngại vật
                    if (cur_val == OBST)
                        grid[row, col] = EMPTY; // Xóa chướng ngại vật
                                                // Khởi tạo lại thuật toán Dijkstra nếu đang chọn thuật toán Dijkstra và là thời gian thực
                    if (realTime && dijkstra.Checked)
                        InitializeDijkstra();
                }
                if (realTime)
                    RealTime_action(); // Thực hiện hành động thời gian thực
                else
                    Invalidate(); // Vẽ lại giao diện
            }
        }
        // Kết thúc MainForm_MouseDown



        /// Xử lý sự kiện di chuyển chuột để "vẽ" chướng ngại vật hoặc di chuyển robot và/hoặc mục tiêu.
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mouse_down)
                return; // Nếu chuột chưa được nhấn xuống thì không làm gì
            int row = (e.Y - 10) / squareSize;  // Xác định hàng dựa trên vị trí Y của chuột
            int col = (e.X - 10) / squareSize;  // Xác định cột dựa trên vị trí X của chuột
            if (row >= 0 && row < rows && col >= 0 && col < columns)
            {
                // Chỉ cho phép thao tác nếu là thời gian thực hoặc chưa tìm thấy và không đang tìm kiếm
                if (realTime ? true : !found && !searching)
                {
                    if (realTime)
                        FillGrid(); // Đổ lại lưới nếu là thời gian thực

                    // Xử lý di chuyển robot hoặc mục tiêu
                    if (!(row == cur_row && col == cur_col) && (cur_val == ROBOT || cur_val == TARGET))
                    {
                        int new_val = grid[row, col];
                        if (new_val == EMPTY)
                        {
                            grid[row, col] = cur_val;
                            if (cur_val == ROBOT)
                            {
                                robotStart.row = row;
                                robotStart.col = col;
                            }
                            else
                            {
                                targetPos.row = row;
                                targetPos.col = col;
                            }
                            grid[cur_row, cur_col] = new_val;
                            cur_row = row;
                            cur_col = col;
                            cur_val = grid[row, col];
                        }
                    }
                    // Nếu không di chuyển robot hoặc mục tiêu thì vẽ chướng ngại vật
                    else if (grid[row, col] != ROBOT && grid[row, col] != TARGET)
                        grid[row, col] = OBST;
                    // Khởi tạo lại thuật toán Dijkstra nếu đang chọn thuật toán Dijkstra và là thời gian thực
                    if (realTime && dijkstra.Checked)
                        InitializeDijkstra();
                }
                if (realTime)
                    RealTime_action(); // Thực hiện hành động thời gian thực
                else
                    Invalidate(); // Vẽ lại giao diện 
            }
        }
        // Kết thúc MainForm_MouseMove



        /// Khi người dùng nhả chuột ra
        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            mouse_down = false;  // Đặt lại biến chuột đã nhấn xuống về false
        }
        // Kết thúc MainForm_MouseUp

        /// <summary>
        /// Tạo một lưới mới hoặc một mê cung mới
        /// </summary>
        /// <param name="makeMaze">Cờ chỉ định cho việc tạo mê cung</param>
        private void InitializeGrid(bool makeMaze)
        {
            level = 0; // Đặt mức độ về 0
            rows = (int)this.rowsSpinner.Value; // Số hàng lấy từ giá trị của rowsSpinner
            columns = (int)this.columnsSpinner.Value; // Số cột lấy từ giá trị của columnsSpinner
            // Mê cung phải có số hàng và số cột là số lẻ
            if (makeMaze && rows % 2 == 0)
                rows -= 1;
            if (makeMaze && columns % 2 == 0)
                columns -= 1;
            // Xác định kích thước của mỗi ô
            squareSize = (int)(500 / (rows > columns ? rows : columns));
            arrowSize = (int)(squareSize / 2); 
            grid = new int[rows, columns];
            robotStart = new Cell(rows - 2, 1);  // Vị trí bắt đầu của robot
            targetPos = new Cell(1, columns - 2);  // Vị trí đích đến
            FillGrid(); // Điền dữ liệu vào lưới
            if (makeMaze)
            {
                MyMaze maze = new MyMaze(rows / 2, columns / 2); // Tạo đối tượng mê cung
                for (int x = 0; x < maze.gridDimensionX; x++)
                    for (int y = 0; y < maze.gridDimensionY; y++)
                        if (maze.mazeGrid[x, y] == 'X')
                            grid[x, y] = OBST; // Đánh dấu các ô là chướng ngại vật


            }
            Invalidate(); // Làm mới lưới để vẽ lại
            hintDisplayed = false; // Đặt lại cờ gợi ý mỗi khi khởi tạo lưới

        }
        // Kết thúc InitializeGrid

        /// <summary>
        /// Cung cấp các giá trị ban đầu cho các ô trong lưới.
        /// </summary>
        private void FillGrid()
        {
            /*
            * Khi nhấn vào nút 'Clear' lần đầu tiên, dữ liệu của bất kỳ tìm kiếm nào đã được thực hiện
            * (Frontier, Closed Set, Route) sẽ được xóa bỏ và giữ nguyên các chướng ngại vật cũng như vị trí
            * của robot và mục tiêu để có thể chạy thuật toán khác
            * với cùng dữ liệu.
            * Khi nhấn vào lần thứ hai, cũng sẽ xóa bỏ các chướng ngại vật.
            */
            if (searching || endOfSearch || realTime)
            {
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < columns; c++)
                    {
                        if (grid[r, c] == FRONTIER || grid[r, c] == CLOSED || grid[r, c] == ROUTE)
                            grid[r, c] = EMPTY;
                        if (grid[r, c] == ROBOT)
                            robotStart = new Cell(r, c);
                        if (grid[r, c] == TARGET)
                            targetPos = new Cell(r, c);
                    }
            }
            else
            {
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < columns; c++)
                        grid[r, c] = EMPTY;
                robotStart = new Cell(rows - 2, 1);
                targetPos = new Cell(1, columns - 2);
            }

            // Nếu đang sử dụng thuật toán A* hoặc Greedy, đặt lại các thuộc tính của robotStart
            if (aStar.Checked || greedy.Checked)
            {
                robotStart.g = 0;
                robotStart.h = 0;
                robotStart.f = 0;
            }
            expanded = 0;
            found = false;
            searching = (realTime ? true : false);
            endOfSearch = false;

            // Bước đầu tiên của các thuật toán DFS, BFS, A* và Greedy được thực hiện ở đây
            // 1. OPEN SET: = [So], CLOSED SET: = []
            openSet.Clear();
            openSet.Add(robotStart);
            closedSet.Clear();

            if (!realTime)
            {
                grid[targetPos.row, targetPos.col] = TARGET;
                grid[robotStart.row, robotStart.col] = ROBOT;
            }
        } // end FillGrid


        /// Cho phép các nút radio và các ô đánh dấu được chọn.
        private void EnableRadiosAndChecks()
        {
            slider.Enabled = true; // Cho phép thanh trượt
            dfs.Enabled = true; // Cho phép DFS
            bfs.Enabled = true; // Cho phép BFS
            aStar.Enabled = true; // Cho phép A*
            greedy.Enabled = true; // Cho phép Greedy
            dijkstra.Enabled = true; // Cho phép Dijkstra
            diagonal.Enabled = true; // Cho phép điagonal
            drawArrows.Enabled = true; // Cho phép vẽ mũi tên
        } // end EnableRadiosAndChecks



        /// Vô hiệu hóa các nút radio và các ô đánh dấu.
        private void DisableRadiosAndChecks()
        {
            slider.Enabled = false; // Vô hiệu hóa thanh trượt
            dfs.Enabled = false; // Vô hiệu hóa DFS
            bfs.Enabled = false; // Vô hiệu hóa BFS
            aStar.Enabled = false; // Vô hiệu hóa A*
            greedy.Enabled = false; // Vô hiệu hóa Greedy
            dijkstra.Enabled = false; // Vô hiệu hóa Dijkstra
            diagonal.Enabled = false; // Vô hiệu hóa điagonal
            drawArrows.Enabled = false; // Vô hiệu hóa vẽ mũi tên
        } // end DisableRadiosAndChecks



        /// Khi người dùng nhấn vào nút "Clear"
        private void ResetButton_Click(object sender, EventArgs e)
        {
            animation = false; // Ngừng hoạt hình
            realTime = false; // Ngừng chế độ thời gian thực
            RealTimeButton.Enabled = true; // Cho phép nút RealTimeButton
            RealTimeButton.ForeColor = Color.Black; // Đặt màu chữ của nút RealTimeButton là đen
            StepButton.Enabled = true; // Cho phép nút StepButton
            AnimationButton.Enabled = true; // Cho phép nút AnimationButton
            EnableRadiosAndChecks(); // Cho phép các nút radio và các ô đánh dấu
            InitializeGrid(false); // Khởi tạo lại lưới mà không tạo mê cung
        } // end  ResetButton_Click

        private Timer challengeTimer;
        private int elapsedTime = 0; // Biến để đếm thời gian

        private void ChallengeTimer_Tick(object sender, EventArgs e)
        {
            elapsedTime++; // Tăng biến đếm thời gian
            UpdateTimeLabel(); // Cập nhật nhãn hiển thị thời gian
        }

        private void UpdateTimeLabel()
        {
            // Cập nhật label để hiển thị thời gian
            TimeAndConf.Text = $"Thời gian: {elapsedTime} giây"; // elapsedTime là biến đếm thời gian
        }

        /// Khi người dùng nhấn vào nút "Maze"
        private void MazeButton_Click(object sender, EventArgs e)
        {
            animation = false; // Ngừng hoạt hình
            realTime = false; // Ngừng chế độ thời gian thực
            RealTimeButton.Enabled = true; // Cho phép nút RealTimeButton
            RealTimeButton.ForeColor = Color.Black; // Đặt màu chữ của nút RealTimeButton là đen
            StepButton.Enabled = true; // Cho phép nút StepButton
            AnimationButton.Enabled = true; // Cho phép nút AnimationButton
            EnableRadiosAndChecks(); // Cho phép các nút radio và các ô đánh dấu
            InitializeGrid(true); // Khởi tạo lưới và tạo mê cung

            // Bắt đầu đếm giờ
            challengeTimer.Start(); // Bắt đầu đồng hồ đếm
            UpdateTimeLabel(); // Cập nhật nhãn hiển thị thời gian
        }
        // end MazeButton_Click



        /// Khi người dùng nhấn vào nút "Clear"
        private void ClearButton_Click(object sender, EventArgs e)
        {
            animation = false; // Ngừng hoạt hình nếu đang chạy.
            realTime = false; // Ngừng chế độ thời gian thực nếu đang được kích hoạt.
            RealTimeButton.Enabled = true; // Cho phép nút RealTimeButton để có thể sử dụng lại.
            RealTimeButton.ForeColor = Color.Black; // Đặt màu chữ của nút RealTimeButton thành đen để chỉ ra rằng nút đã được kích hoạt.
            StepButton.Enabled = true; // Cho phép nút StepButton.
            AnimationButton.Enabled = true; // Cho phép nút AnimationButton.
            EnableRadiosAndChecks(); // Cho phép các nút radio và các ô đánh dấu.
            FillGrid(); // Điền dữ liệu vào lưới.
            Invalidate(); // Làm mới lưới để vẽ lại.
        } // end ClearButton_Click



        /// Khi người dùng nhấn vào nút "Real-Time"
        private void RealTimeButton_Click(object sender, EventArgs e)
        {
            if (realTime) // Nếu đang ở chế độ thời gian thực rồi thì không làm gì cả
                return;
            realTime = true;    // Đặt chế độ thời gian thực là true
            searching = true;   // Đang trong quá trình tìm kiếm
                // Khởi tạo Dijkstra chỉ nên được thực hiện ngay trước khi bắt đầu tìm kiếm,
                // bởi vì các chướng ngại vật phải ở đúng vị trí.
            if (dijkstra.Checked)     // Nếu đang chọn thuật toán Dijkstra
                InitializeDijkstra(); // Khởi tạo thuật toán Dijkstra
            RealTimeButton.ForeColor = Color.Red;  // Đặt màu chữ của nút RealTimeButton thành màu đỏ
            DisableRadiosAndChecks();              // Vô hiệu hóa các nút radio và các ô đánh dấu
            RealTime_action();                     // Thực hiện hành động thời gian thực
        } // end RealTimeButton_Click



        /// Hành động thực hiện trong quá trình tìm kiếm thời gian thực
        private void RealTime_action()
        {
            do
                CheckTermination(); // Kiểm tra điều kiện dừng
            while (!endOfSearch);   // Lặp lại cho đến khi kết thúc tìm kiếm
        } // end RealTime_action



        /// Khi người dùng nhấn vào nút "Step-by-Step"
        private void StepButton_Click(object sender, EventArgs e)
        {
            animation = false;  // Ngừng hoạt hình nếu đang chạy.
            timer.Stop();       // Dừng đồng hồ đếm nếu đang chạy.

            // Nếu đã tìm thấy đích hoặc đã kết thúc tìm kiếm, thoát khỏi hàm.
            if (found || endOfSearch)
                return;
            // Nếu không đang trong quá trình tìm kiếm và đang chọn thuật toán Dijkstra,
            // khởi tạo thuật toán Dijkstra.
            if (!searching && dijkstra.Checked)
                InitializeDijkstra();

            searching = true; // Đặt biến searching là true để báo hiệu đang trong quá trình tìm kiếm.
            RealTimeButton.Enabled = false; // Vô hiệu hóa nút RealTimeButton.
            DisableRadiosAndChecks(); // Vô hiệu hóa các nút radio và các ô đánh dấu.
            slider.Enabled = true; // Cho phép thanh trượt để người dùng tương tác.
            CheckTermination(); // Kiểm tra điều kiện dừng của quá trình tìm kiếm.
            Invalidate(); // Làm mới lưới để vẽ lại.
        } // end StepButton_Click



        /// Khi người dùng nhấn vào nút "Animation"
        private void AnimationButton_Click(object sender, EventArgs e)
        {
            animation = true;  // Bật chế độ hoạt hình
            if (!searching && dijkstra.Checked) // Nếu không đang trong quá trình tìm kiếm và đang chọn thuật toán Dijkstra
                InitializeDijkstra();     // Khởi tạo thuật toán Dijkstra
            searching = true;      // Đặt biến searching là true để báo hiệu đang trong quá trình tìm kiếm
            RealTimeButton.Enabled = false; // Vô hiệu hóa nút RealTimeButton
            DisableRadiosAndChecks(); // Vô hiệu hóa các nút radio và các ô đánh dấu
            slider.Enabled = true; // Cho phép thanh trượt để người dùng tương tác
            delay = slider.Value; // Lấy giá trị của thanh trượt làm độ trễ
            timer.Stop(); // Dừng đồng hồ đếm nếu đang chạy
            timer.Interval = delay; // Đặt thời gian độ trễ cho đồng hồ đếm
            timer.Start(); // Khởi động lại đồng hồ đếm để bắt đầu hoạt động
            Animation_action(); // Thực hiện hành động hoạt động
        } // end AnimationButton_Click



        /// Hành động thực hiện trong quá trình tìm kiếm động
        private void Animation_action()
        {
            if (animation) // Nếu đang trong chế độ hoạt hình
            {
                CheckTermination(); // Kiểm tra điều kiện dừng của quá trình tìm kiếm
                Invalidate(); // Làm mới lưới để vẽ lại
                if (endOfSearch) // Nếu đã kết thúc tìm kiếm
                {
                    animation = false; // Tắt chế độ hoạt hình
                    timer.Stop(); // Dừng đồng hồ đếm
                    isMessageShown = false; // Thiết lập lại cờ để cho phép thông báo trong lần chạy tiếp theo
                }
            }
        } // end Animation_action

        /// Hành động thực hiện sau mỗi khoảng thời gian của đồng hồ đếm
        private void timer_Tick(object sender, EventArgs e)
        {
            Animation_action(); // Thực hiện hành động hoạt hình
        } // end Timer_Tick

        private void rowsLbl_Click(object sender, EventArgs e)
        {

        }

        private void closedLbl_Click(object sender, EventArgs e)
        {

        }

        private void frontierLbl_Click(object sender, EventArgs e)
        {

        }

        private void targetLbl_Click(object sender, EventArgs e)
        {

        }

        private void robotLbl_Click(object sender, EventArgs e)
        {

        }

        private void ToolTip_Popup(object sender, PopupEventArgs e)
        {

        }

        private void message_Click(object sender, EventArgs e)
        {

        }


        private void columnsSpinner_ValueChanged(object sender, EventArgs e)
        {

        }

        private void columnsLbl_Click(object sender, EventArgs e)
        {

        }

        private void rowsSpinner_ValueChanged(object sender, EventArgs e)
        {

        }



        /// Khi người dùng nhấn vào nút "Thông tin về Mê cung"
        private void AboutButton_Click(object sender, EventArgs e)
        {
            AboutBox aboutForm = new AboutBox(); // Tạo một đối tượng form AboutBox
            aboutForm.ShowDialog(); // Hiển thị form AboutBox dưới dạng hộp thoại modal
        } // end AboutButton_Click



        /// Kiểm tra xem chúng ta đã đạt đến cuối quá trình tìm kiếm chưa
        private void CheckTermination()
        {
            // Ở đây chúng ta quyết định liệu có thể tiếp tục tìm kiếm hay không.

            // Trong trường hợp thuật toán Dijkstra
            // ở đây chúng ta kiểm tra điều kiện của bước 11:
            // 11. trong khi Q không rỗng.

            // Trong trường hợp các thuật toán DFS, BFS, A* và Greedy
            // ở đây chúng ta có bước thứ hai:
            // 2. Nếu OPEN SET = [], thì kết thúc. Không có giải pháp.
            if ((dijkstra.Checked && graph.Count == 0) ||
                          (!dijkstra.Checked && openSet.Count == 0))
            {
                endOfSearch = true; // Đã đến cuối quá trình tìm kiếm
                grid[robotStart.row, robotStart.col] = ROBOT; // Đặt lại vị trí của robot
                StepButton.Enabled = false; // Vô hiệu hóa nút StepButton
                AnimationButton.Enabled = false; // Vô hiệu hóa nút AnimationButton
                Invalidate(); // Làm mới lưới để vẽ lại
            }
            else
            {
                ExpandNode(); // Mở rộng nút
                if (found) 
                {
                    endOfSearch = true; // Đã tìm thấy đích
                    PlotRoute(); // Vẽ đường đi
                    StepButton.Enabled = false; // Vô hiệu hóa nút StepButton
                    AnimationButton.Enabled = false; // Vô hiệu hóa nút AnimationButton
                    slider.Enabled = false; // Vô hiệu hóa thanh trượt
                    Invalidate(); // Làm mới lưới để vẽ lại
                }
            }
        } // end CheckTermination



        /// Mở rộng một nút và tạo các nút kế tiếp của nó
        private void ExpandNode()
        {
            if (dijkstra.Checked) // Xử lý riêng cho thuật toán Dijkstra
            {

                if (graph.Count == 0)  // 11: trong khi Q không rỗng
                    return;
                // 12: u := đỉnh trong Q (graph) có khoảng cách nhỏ nhất trong dist[]
                // 13: loại bỏ u khỏi Q (graph)
                Cell u = (Cell)graph[0];
                graph.RemoveAt(0);
                // Thêm đỉnh u vào tập CLOSED SET
                closedSet.Add(u);
                // Nếu tìm thấy mục tiêu ...
                if (u.row == targetPos.row && u.col == targetPos.col)
                {
                    found = true;
                    return;
                }
                // Đếm số nút đã mở rộng
                expanded++;
                // Cập nhật màu của ô
                grid[u.row, u.col] = CLOSED;
                // 14: nếu dist[u] = vô cùng
                if (u.dist == INFINITY)
                {
                    // Nếu khoảng cách từ điểm u đến đích là vô cùng, tức là không có lời giải.
                    // 15: break;
                    return;
                } // 16: end if
                // Tạo các đỉnh kề của u
                List<Cell> neighbors = CreateSuccesors(u, false);
                // 18: for each neighbor v of u:
                foreach (Cell v in neighbors)
                {
                    // 20: alt := dist[u] + dist_between(u, v) ;
                    double alt = u.dist + DistBetween(u, v);
                    // 21: if alt < dist[v]:
                    if (alt < v.dist)
                    {
                        // 22: dist[v] := alt ;
                        v.dist = alt;
                        // 23: previous[v] := u ;
                        v.prev = u;
                        // Update the color of the cell
                        grid[v.row, v.col] = FRONTIER;
                        // 24: decrease-key v in Q;
                        // (sắp xếp danh sách các đỉnh theo thứ tự 'dist' và 'level')
                        graph.Sort((x, y) =>
                        {
                            int result = x.dist.CompareTo(y.dist);
                            if (result == 0)
                                result = x.level.CompareTo(y.level);
                            return result;
                        });
                    }
                }
            }
            else // Xử lý các thuật toán còn lại
            {
                Cell current;
                if (dfs.Checked || bfs.Checked)
                {
                    // Đây là bước thứ 3 của thuật toán DFS và BFS
                    // 3. Loại bỏ trạng thái đầu tiên, Si, từ OPEN SET ...
                    current = (Cell)openSet[0];
                    openSet.RemoveAt(0);
                }
                else
                {
                    // Đây là bước thứ 3 của thuật toán A* và Greedy
                    // 3. Loại bỏ trạng thái đầu tiên, Si, từ OPEN SET,
                    // sao cho f(Si) ≤ f(Sj) đối với tất cả các trạng thái Sj khác
                    // trong OPEN SET ...
                    // (sắp xếp danh sách OPEN SET theo 'f' và 'level')
                    openSet.Sort((x, y) =>
                    {
                        int result = x.f.CompareTo(y.f);
                        if (result == 0)
                            result = x.level.CompareTo(y.level);
                        return result;
                    });

                    current = (Cell)openSet[0];
                    openSet.RemoveAt(0);
                }
                // ... và thêm nó vào CLOSED SET.
                closedSet.Insert(0, current);
                // Cập nhật màu sắc của ô
                grid[current.row, current.col] = CLOSED;
                // Nếu nút được chọn là mục tiêu ...
                if (current.row == targetPos.row && current.col == targetPos.col)
                {
                    // ... thì kết thúc quá trình và cập nhật ...
                    Cell last = targetPos;
                    last.prev = current.prev;
                    closedSet.Add(last);
                    found = true;
                    return;
                }
                // Đếm số nút đã được mở rộng.
                expanded++;
                // Đây là bước thứ 4 của các thuật toán
                // 4. Tạo các nút kế tiếp của Si, dựa trên các hành động
                //    có thể thực hiện được trên Si.
                //    Mỗi nút kế tiếp có con trỏ đến Si, là nút tiền nhiệm của nó.
                //    Trong trường hợp của các thuật toán DFS và BFS, các nút kế tiếp không nên
                //    thuộc OPEN SET hoặc CLOSED SET.
                List<Cell> succesors = CreateSuccesors(current, false);
                // Đây là bước thứ 5 của các thuật toán
                // 5. Đối với mỗi nút kế tiếp của Si, ...
                foreach (Cell cell in succesors)
                {
                    if (dfs.Checked) // ... nếu đang chạy DFS ...
                    {
                        // ... thêm nút kế tiếp vào đầu danh sách OPEN SET
                        openSet.Insert(0, cell);
                        // Cập nhật màu sắc của ô
                        grid[cell.row, cell.col] = FRONTIER;
                        // ... if we are runnig BFS ...
                    }
                    else if (bfs.Checked)
                    {
                        // ... thêm nút kế tiếp vào cuối danh sách OPEN SET
                        openSet.Add(cell);
                        // Cập nhật màu sắc của ô
                        grid[cell.row, cell.col] = FRONTIER;
                    }
                    // ... if we are running A* or Greedy algorithms (step 5 of A* algorithm) ...
                    else if (aStar.Checked || greedy.Checked)
                    {
                        // ... tính giá trị f(Sj) ...
                        int dxg = current.col - cell.col;
                        int dyg = current.row - cell.row;
                        int dxh = targetPos.col - cell.col;
                        int dyh = targetPos.row - cell.row;
                        if (diagonal.Checked)
                        {
                            // với các chuyển động chéo
                            // tính khoảng cách Euclid
                            if (greedy.Checked)
                            {
                                // đặc biệt cho Greedy ...
                                cell.g = 0;
                            }
                            else
                            {
                                cell.g = current.g + Math.Sqrt(dxg * dxg + dyg * dyg);
                            }
                            cell.h = Math.Sqrt(dxh * dxh + dyh * dyh);
                        }
                        else
                        {
                            // không có các chuyển động chéo
                            // tính khoảng cách Manhattan
                            if (greedy.Checked)
                            {
                                // đặc biệt cho Greedy ...
                                cell.g = 0;
                            }
                            else
                            {
                                cell.g = current.g + Math.Abs(dxg) + Math.Abs(dyg);
                            }
                            cell.h = Math.Abs(dxh) + Math.Abs(dyh);
                        }
                        cell.f = cell.g + cell.h;
                        int openIndex = IsInList(openSet, cell);
                        int closedIndex = IsInList(closedSet, cell);
                        // ... Nếu Sj không thuộc OPEN SET hoặc CLOSED SET ...
                        if (openIndex == -1 && closedIndex == -1)
                        {
                            // ... thêm Sj vào OPEN SET ...
                            // ... được đánh giá là f(Sj)
                            openSet.Add(cell);
                            // Cập nhật màu sắc của ô
                            grid[cell.row, cell.col] = FRONTIER;
                            // Else ...
                        }
                        else
                        {
                            // ... nếu đã thuộc OPEN SET, thì ...
                            if (openIndex > -1)
                            {
                                // ... so sánh giá trị đánh giá mới với giá trị cũ.
                                // If old <= new ...
                                Cell openSetCell = (Cell)openSet[openIndex];
                                if (openSetCell.f <= cell.f)
                                {
                                    // ... thì bỏ qua nút mới với trạng thái Sj.
                                    // (nghĩa là không làm gì với nút này).
                                    // Else, ...
                                }
                                else
                                {
                                    // ... loại bỏ phần tử (Sj, cũ) khỏi danh sách
                                    // mà nó thuộc về ...
                                    openSet.RemoveAt(openIndex);
                                    // ... và thêm mục (Sj, mới) vào OPEN SET.
                                    openSet.Add(cell);
                                    // Cập nhật màu sắc của ô
                                    grid[cell.row, cell.col] = FRONTIER;
                                }
                                // ... nếu đã thuộc CLOSED SET, thì ...
                            }
                            else
                            {
                                // ... so sánh giá trị đánh giá mới với giá trị cũ.
                                // If old <= new ...
                                Cell closedSetCell = (Cell)closedSet[closedIndex];
                                if (closedSetCell.f <= cell.f)
                                {
                                    // ... thì bỏ qua nút mới với trạng thái Sj.
                                    // (nghĩa là không làm gì với nút này).
                                    // Else, ...
                                }
                                else
                                {
                                    // ... loại bỏ phần tử (Sj, cũ) khỏi danh sách
                                    // mà nó thuộc về ...
                                    closedSet.RemoveAt(closedIndex);
                                    // ... và thêm mục (Sj, mới) vào OPEN SET.
                                    openSet.Add(cell);
                                    // Cập nhật màu sắc của ô
                                    grid[cell.row, cell.col] = FRONTIER;
                                }
                            }
                        }
                    }
                }
            }
        } //end ExpandNode()

        /// <summary>
        /// Tạo ra các ô kế tiếp của một ô/cell.
        /// </summary>
        /// <param name="current">Ô/cell hiện tại mà chúng ta đang tạo các ô kế tiếp.</param>
        /// <param name="makeConnected">
        /// Cờ chỉ ra rằng chúng ta chỉ quan tâm đến tọa độ của các ô và không quan tâm đến nhãn 'dist' 
        /// (liên quan chỉ đến thuật toán Dijkstra).</param>
        /// <returns>Danh sách các ô kế tiếp của ô hiện tại.</returns>
        private List<Cell> CreateSuccesors(Cell current, bool makeConnected)
        {
            int r = current.row;
            int c = current.col;
            // Tạo ra một danh sách rỗng để chứa các ô kế tiếp của ô hiện tại.
            List<Cell> temp = new List<Cell>();
            // Với các chuyển động theo đường chéo, ưu tiên là:
            // 1: Lên 2: Lên-phải 3: Phải 4: Xuống-phải
            // 5: Xuống 6: Xuống-trái 7: Trái 8: Lên-trái

            // Nếu không có chuyển động theo đường chéo, ưu tiên là:
            // 1: Lên 2: Phải 3: Xuống 4: Trái

            // Nếu không ở giới hạn trên cùng của lưới và ô phía trên không phải là chướng ngại vật ...
            if (r > 0 && grid[r - 1, c] != OBST &&
                    // ... và (chỉ trong trường hợp không chạy A* hoặc Greedy)
                    // không thuộc cả tập openSet và closedSet ...
                    ((aStar.Checked || greedy.Checked || dijkstra.Checked) ? true :
                          IsInList(openSet, new Cell(r - 1, c)) == -1 &&
                          IsInList(closedSet, new Cell(r - 1, c)) == -1))
            {
                Cell cell = new Cell(r - 1, c);
                // Trong trường hợp thuật toán Dijkstra, không thể thêm ô "trần trụi" vào danh sách các ô kế tiếp.
                // Ô này phải được kèm theo nhãn 'dist', do đó chúng ta cần theo dõi nó qua danh sách 'graph'
                // và sao chép lại vào danh sách các ô kế tiếp.
                // Cờ makeConnected là cần thiết để phương thức createSuccesors() hợp tác
                // với phương thức findConnectedComponent(), tạo ra thành phần liên thông khi Dijkstra khởi tạo.
                if (dijkstra.Checked)
                {
                    if (makeConnected)
                        temp.Add(cell);
                    else
                    {
                        int graphIndex = IsInList(graph, cell);
                        if (graphIndex > -1)
                        {
                            graph[graphIndex].level = ++level;
                            temp.Add(graph[graphIndex]);
                        }
                    }
                }
                else
                {
                    // ... cập nhật con trỏ của ô phía trên để trỏ vào ô hiện tại ...
                    cell.prev = current;
                    // ... và thêm ô phía trên vào danh sách các ô kế tiếp của ô hiện tại.                    cell.level = ++level;
                    temp.Add(cell);
                }
            }
            if (diagonal.Checked)
            {
                // Nếu không phải ở cả biên trên cùng và biên phải nhất của lưới
                // và ô phía trên-bên phải không phải là chướng ngại vật ...
                if (r > 0 && c < columns - 1 && grid[r - 1, c + 1] != OBST &&
                        // ... và một trong các ô phía trên hoặc bên phải không phải là chướng ngại vật ...
                        // (bởi vì không hợp lý để cho phép robot đi qua một "khe hở")                       
                        (grid[r - 1, c] != OBST || grid[r, c + 1] != OBST) &&
                        // ... và (chỉ trong trường hợp không chạy A* hoặc Greedy)
                        // không thuộc cả tập openSet và closedSet ...
                        ((aStar.Checked || greedy.Checked || dijkstra.Checked) ? true :
                              IsInList(openSet, new Cell(r - 1, c + 1)) == -1 &&
                              IsInList(closedSet, new Cell(r - 1, c + 1)) == -1))
                {
                    Cell cell = new Cell(r - 1, c + 1);
                    if (dijkstra.Checked)
                    {
                        if (makeConnected)
                            temp.Add(cell);
                        else
                        {
                            int graphIndex = IsInList(graph, cell);
                            if (graphIndex > -1)
                            {
                                graph[graphIndex].level = ++level;
                                temp.Add(graph[graphIndex]);
                            }
                        }
                    }
                    else
                    {
                        // ... cập nhật con trỏ của ô phía trên-bên phải để trỏ đến ô hiện tại ...
                        cell.prev = current;
                        // ... và thêm ô phía trên-bên phải vào danh sách các ô kế cận của ô hiện tại. 
                        cell.level = ++level;
                        temp.Add(cell);
                    }
                }
            }
            // Nếu không phải ở biên phải nhất của lưới
            // và ô phía bên phải không phải là chướng ngại vật ...
            if (c < columns - 1 && grid[r, c + 1] != OBST &&
                    // ... và (chỉ trong trường hợp không chạy thuật toán A* hoặc Greedy)
                    // không thuộc OPEN SET hoặc CLOSED SET ...
                    ((aStar.Checked || greedy.Checked || dijkstra.Checked) ? true :
                          IsInList(openSet, new Cell(r, c + 1)) == -1 &&
                          IsInList(closedSet, new Cell(r, c + 1)) == -1))
            {
                Cell cell = new Cell(r, c + 1);
                if (dijkstra.Checked)
                {
                    if (makeConnected)
                        temp.Add(cell);
                    else
                    {
                        int graphIndex = IsInList(graph, cell);
                        if (graphIndex > -1)
                        {
                            graph[graphIndex].level = ++level;
                            temp.Add(graph[graphIndex]);
                        }
                    }
                }
                else
                {
                    // ... cập nhật con trỏ của ô phía bên phải để trỏ đến ô hiện tại ...
                    cell.prev = current;
                    // ... và thêm ô phía bên phải vào danh sách các ô kế cận của ô hiện tại. 
                    cell.level = ++level;
                    temp.Add(cell);
                }
            }
            if (diagonal.Checked)
            {
                // Nếu không phải ở biên dưới cùng hoặc biên phải nhất của lưới
                // và ô phía dưới bên phải không phải là chướng ngại vật ...
                if (r < rows - 1 && c < columns - 1 && grid[r + 1, c + 1] != OBST &&
                        // ... và một trong số các ô phía dưới hoặc bên phải không phải là chướng ngại vật ...
                        (grid[r + 1, c] != OBST || grid[r, c + 1] != OBST) &&
                        // ... và (chỉ trong trường hợp không chạy thuật toán A* hoặc Greedy)
                        // không thuộc OPEN SET hoặc CLOSED SET ...
                        ((aStar.Checked || greedy.Checked || dijkstra.Checked) ? true :
                              IsInList(openSet, new Cell(r + 1, c + 1)) == -1 &&
                              IsInList(closedSet, new Cell(r + 1, c + 1)) == -1))
                {
                    Cell cell = new Cell(r + 1, c + 1);
                    if (dijkstra.Checked)
                    {
                        if (makeConnected)
                            temp.Add(cell);
                        else
                        {
                            int graphIndex = IsInList(graph, cell);
                            if (graphIndex > -1)
                            {
                                graph[graphIndex].level = ++level;
                                temp.Add(graph[graphIndex]);
                            }
                        }
                    }
                    else
                    {
                        // ... cập nhật con trỏ của ô phía dưới bên phải để trỏ đến ô hiện tại ...
                        cell.prev = current;
                        // ... và thêm ô phía dưới bên phải vào danh sách các ô kế cận của ô hiện tại. 
                        cell.level = ++level;
                        temp.Add(cell);
                    }
                }
            }
            // Nếu không phải là ở biên dưới cùng của lưới
            // và ô phía dưới không phải là chướng ngại vật ...
            if (r < rows - 1 && grid[r + 1, c] != OBST &&
                    // ... và (chỉ trong trường hợp không chạy thuật toán A* hoặc Greedy)
                    // không thuộc OPEN SET hoặc CLOSED SET ...
                    ((aStar.Checked || greedy.Checked || dijkstra.Checked) ? true :
                          IsInList(openSet, new Cell(r + 1, c)) == -1 &&
                          IsInList(closedSet, new Cell(r + 1, c)) == -1))
            {
                Cell cell = new Cell(r + 1, c);
                if (dijkstra.Checked)
                {
                    if (makeConnected)
                        temp.Add(cell);
                    else
                    {
                        int graphIndex = IsInList(graph, cell);
                        if (graphIndex > -1)
                        {
                            graph[graphIndex].level = ++level;
                            temp.Add(graph[graphIndex]);
                        }
                    }
                }
                else
                {
                    // ... cập nhật con trỏ của ô phía dưới để trỏ đến ô hiện tại ...
                    cell.prev = current;
                    // ... và thêm ô phía dưới vào danh sách các ô kế cận của ô hiện tại. 
                    cell.level = ++level;
                    temp.Add(cell);
                }
            }
            if (diagonal.Checked)
            {
                // Nếu không phải ở biên dưới cùng hoặc biên trái cùng của lưới
                // và ô phía dưới bên trái không phải là chướng ngại vật ...
                if (r < rows - 1 && c > 0 && grid[r + 1, c - 1] != OBST &&
                        // ... và một trong số các ô phía dưới hoặc bên trái không phải là chướng ngại vật ...
                        (grid[r + 1, c] != OBST || grid[r, c - 1] != OBST) &&
                        // ... và (chỉ trong trường hợp không chạy thuật toán A* hoặc Greedy)
                        // không thuộc OPEN SET hoặc CLOSED SET ...
                        ((aStar.Checked || greedy.Checked || dijkstra.Checked) ? true :
                              IsInList(openSet, new Cell(r + 1, c - 1)) == -1 &&
                              IsInList(closedSet, new Cell(r + 1, c - 1)) == -1))
                {
                    Cell cell = new Cell(r + 1, c - 1);
                    if (dijkstra.Checked)
                    {
                        if (makeConnected)
                            temp.Add(cell);
                        else
                        {
                            int graphIndex = IsInList(graph, cell);
                            if (graphIndex > -1)
                            {
                                graph[graphIndex].level = ++level;
                                temp.Add(graph[graphIndex]);
                            }
                        }
                    }
                    else
                    {
                        // ... cập nhật con trỏ của ô phía dưới bên trái để trỏ đến ô hiện tại ...
                        cell.prev = current;
                        // ... và thêm ô phía dưới bên trái vào danh sách các ô kế cận của ô hiện tại. 
                        cell.level = ++level;
                        temp.Add(cell);
                    }
                }
            }
            // Nếu không phải là ở biên trái cùng của lưới
            // và ô phía bên trái không phải là chướng ngại vật ...
            if (c > 0 && grid[r, c - 1] != OBST &&
                    // ... và (chỉ trong trường hợp không chạy thuật toán A* hoặc Greedy)
                    // không thuộc OPEN SET hoặc CLOSED SET ...
                    ((aStar.Checked || greedy.Checked || dijkstra.Checked) ? true :
                          IsInList(openSet, new Cell(r, c - 1)) == -1 &&
                          IsInList(closedSet, new Cell(r, c - 1)) == -1))
            {
                Cell cell = new Cell(r, c - 1);
                if (dijkstra.Checked)
                {
                    if (makeConnected)
                        temp.Add(cell);
                    else
                    {
                        int graphIndex = IsInList(graph, cell);
                        if (graphIndex > -1)
                        {
                            graph[graphIndex].level = ++level;
                            temp.Add(graph[graphIndex]);
                        }
                    }
                }
                else
                {
                    // ... cập nhật con trỏ của ô bên trái để trỏ đến ô hiện tại ...
                    cell.prev = current;
                    // ... và thêm ô bên trái vào danh sách các ô kế cận của ô hiện tại. 
                    cell.level = ++level;
                    temp.Add(cell);
                }
            }
            if (diagonal.Checked)
            {
                // Nếu không phải là ở biên trên cùng hoặc biên trái cùng của lưới
                // và ô phía trên bên trái không phải là chướng ngại vật ...
                if (r > 0 && c > 0 && grid[r - 1, c - 1] != OBST &&
                        // ... và một trong số các ô phía trên hoặc bên trái không phải là chướng ngại vật ...
                        (grid[r - 1, c] != OBST || grid[r, c - 1] != OBST) &&
                        // ... và (chỉ trong trường hợp không chạy thuật toán A* hoặc Greedy)
                        // không thuộc OPEN SET hoặc CLOSED SET ...
                        ((aStar.Checked || greedy.Checked || dijkstra.Checked) ? true :
                              IsInList(openSet, new Cell(r - 1, c - 1)) == -1 &&
                              IsInList(closedSet, new Cell(r - 1, c - 1)) == -1))
                {
                    Cell cell = new Cell(r - 1, c - 1);
                    if (dijkstra.Checked)
                    {
                        if (makeConnected)
                            temp.Add(cell);
                        else
                        {
                            int graphIndex = IsInList(graph, cell);
                            if (graphIndex > -1)
                            {
                                graph[graphIndex].level = ++level;
                                temp.Add(graph[graphIndex]);
                            }
                        }
                    }
                    else
                    {
                        // ... cập nhật con trỏ của ô phía trên bên trái để trỏ đến ô hiện tại ...
                        cell.prev = current;
                        // ... và thêm ô phía trên bên trái vào danh sách các ô kế cận của ô hiện tại. 
                        cell.level = ++level;
                        temp.Add(cell);
                    }
                }
            }
            // Khi thuật toán DFS đang được sử dụng, các ô được thêm một cách tuần tự vào đầu danh sách OPEN SET.
            // Do đó, chúng ta cần đảo ngược thứ tự các ô kế cận được tạo ra,
            // để ô kế cận có độ ưu tiên cao nhất được đặt vào đầu danh sách.
            // Đối với Greedy, A* và Dijkstra, không có vấn đề này, vì danh sách được sắp xếp
            // theo 'f' hoặc 'dist' trước khi lấy phần tử đầu tiên ra.
            temp.Reverse();

            return temp;
        } // end CreateSuccesors()




        /// <summary>
        /// Trả về khoảng cách giữa hai ô
        /// </summary>
        /// <param name="u">Ô đầu tiên</param>
        /// <param name="v">Ô kia</param>
        /// <returns>Khoảng cách giữa hai ô u và v</returns>
        private double DistBetween(Cell u, Cell v)
        {
            double dist;
            int dx = u.col - v.col;
            int dy = u.row - v.row;
            if (diagonal.Checked)
            {
                // Với di chuyển theo đường chéo
                // tính khoảng cách Euclide
                dist = Math.Sqrt(dx * dx + dy * dy);
            }
            else
            {
                // Không có di chuyển theo đường chéo
                // tính khoảng cách Manhattan
                dist = Math.Abs(dx) + Math.Abs(dy);
            }
            return dist;
        } // end DistBetween()

        /// <summary>
        /// Trả về chỉ số của ô 'current' trong danh sách 'list'
        /// </summary>
        /// <param name="list">Danh sách mà chúng ta đang tìm kiếm</param>
        /// <param name="current">Ô mà chúng ta đang tìm</param>
        /// <returns>Chỉ số của ô trong danh sách. Nếu ô không được tìm thấy, trả về -1</returns>
        private int IsInList(List<Cell> list, Cell current)
        {
            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                Cell listItem = (Cell)list[i];
                if (current.row == listItem.row && current.col == listItem.col)
                {
                    index = i;
                    break;
                }
            }
            return index;
        } // end IsInList()

        /// <summary>
        /// Trả về ô tiền nhiệm của ô 'current' trong danh sách 'list'
        /// </summary>
        /// <param name="list">Danh sách mà chúng ta đang tìm kiếm</param>
        /// <param name="current">Ô mà chúng ta đang tìm</param>
        /// <returns>Ô tiền nhiệm của ô 'current'</returns>
        private Cell FindPrev(List<Cell> list, Cell current)
        {
            int index = IsInList(list, current);
            Cell cell = (Cell)list[index];
            return cell.prev;
        } // end FindPrev()



        /// Tính toán đường đi từ vị trí đích đến vị trí ban đầu của robot,
        /// đếm số bước tương ứng và đo khoảng cách đã đi được. <summary>
        /// Tính toán đường đi từ vị trí đích đến vị trí ban đầu của robot,
        /// </summary>
        private bool isMessageShown = false; // Thêm biến này vào lớp MainForm
        private bool hintDisplayed = false; // Thêm biến này vào lớp MainForm

        private void PlotRoute()
        {
            int steps = 0;
            double distance = 0;
            int index = IsInList(closedSet, targetPos);
            Cell cur = (Cell)closedSet[index];
            grid[cur.row, cur.col] = TARGET;

            do
            {
                steps++;
                if (diagonal.Checked)
                {
                    int dx = cur.col - cur.prev.col;
                    int dy = cur.row - cur.prev.row;
                    distance += Math.Sqrt(dx * dx + dy * dy);
                }
                else
                    distance++;

                cur = cur.prev;
                grid[cur.row, cur.col] = ROUTE;
            } while (!(cur.row == robotStart.row && cur.col == robotStart.col));

            grid[robotStart.row, robotStart.col] = ROBOT;

            // Chỉ hiển thị gợi ý đường đi một lần
            
        }

        // end PlotRoute()

        /// <summary>
        /// Thêm vào danh sách chứa các nút của đồ thị chỉ những ô thuộc cùng thành phần liên thông với nút v.
        /// Đây là một duyệt theo chiều rộng của đồ thị bắt đầu từ nút v.
        /// </summary>
        /// <param name="v">Nút bắt đầu</param>
        private void FindConnectedComponent(Cell v)
        {
            Stack<Cell> stack = new Stack<Cell>();
            List<Cell> succesors;
            stack.Push(v);
            graph.Add(v);
            while (!(stack.Count == 0))
            {
                v = stack.Pop();
                succesors = CreateSuccesors(v, true);
                foreach (Cell c in succesors)
                    if (IsInList(graph, c) == -1)
                    {
                        stack.Push(c);
                        graph.Add(c);
                    }
            }
        } // end FindConnectedComponent()

        /// <summary>
        /// Khởi tạo thuật toán Dijkstra
        /// </summary>
        private void InitializeDijkstra()
        {
            /*
             * Khi nghĩ về mã giả trên Wikipedia, ta nhận thấy rằng
             * thuật toán vẫn đang tìm kiếm mục tiêu của mình trong khi vẫn còn
             * các đỉnh trong hàng đợi Q.
             * Chỉ khi chúng ta cạn kiệt hàng đợi và chưa tìm thấy mục tiêu,
             * chúng ta mới có thể cho rằng không có giải pháp.
             * Như đã biết, thuật toán mô hình hóa vấn đề như một đồ thị liên thông.
             * Rõ ràng là không có giải pháp tồn tại chỉ khi đồ thị không
             * liên thông và mục tiêu nằm trong một thành phần liên thông khác
             * so với vị trí ban đầu của robot.
             * Để có thể có phản hồi tiêu cực từ thuật toán,
             * cần tìm kiếm CHỈ trong thành phần liên thông mà
             * vị trí ban đầu của robot thuộc về.
             */

            // Đầu tiên tạo thành phần liên thông
            // mà vị trí ban đầu của robot thuộc về.
            graph.Clear();
            FindConnectedComponent(robotStart);
            // Đây là sự khởi tạo của thuật toán Dijkstra
            // 2: for each vertex v in Graph;
            foreach (Cell v in graph)
            {
                // 3: dist[v] := infinity ;
                v.dist = INFINITY;
                // 5: previous[v] := undefined ;
                v.prev = null;
            }
            // 8: dist[source] := 0;
            graph[IsInList(graph, robotStart)].dist = 0;
            // 9: Q := the set of all nodes in Graph;
            // Thay vì biến Q, chúng ta sẽ sử dụng danh sách
            // 'graph' chính nó, đã được khởi tạo sẵn.           

            // Sắp xếp danh sách các đỉnh theo 'dist'.
            graph.Sort((r1, r2) => r1.dist.CompareTo(r2.dist));
            // Khởi tạo danh sách các đỉnh đã đóng
            closedSet.Clear();
        } // end InitializeDijkstra

        private void MainForm_Load(object sender, EventArgs e)
        {

        }


        // Hàm di chuyển robot
        private bool MoveRobot(int deltaRow, int deltaCol)
        {
            int newRow = robotStart.row + deltaRow;
            int newCol = robotStart.col + deltaCol;

            // Kiểm tra vị trí mới có hợp lệ không
            if (newRow >= 0 && newRow < rows && newCol >= 0 && newCol < columns && grid[newRow, newCol] != OBST)
            {
                // Cập nhật vị trí của robot
                grid[robotStart.row, robotStart.col] = EMPTY; // Đặt ô cũ thành trống
                robotStart.row = newRow; // Cập nhật hàng mới
                robotStart.col = newCol; // Cập nhật cột mới
                grid[robotStart.row, robotStart.col] = ROBOT; // Đặt robot ở vị trí mới
                Invalidate(); // Làm mới giao diện

                // Kiểm tra xem robot có đến đích không
                if (robotStart.row == targetPos.row && robotStart.col == targetPos.col)
                {
                    challengeTimer.Stop(); // Dừng đồng hồ đếm
                    // Tạo mảng thông báo chúc mừng
                    string[] messages = new string[]
                    {
                $"Chúc mừng! Bạn đã hoàn thành trong {elapsedTime} giây!",
                $"Xin chúc mừng! Robot đã tìm thấy đường đến đích chỉ trong {elapsedTime} giây!",
                $"Tuyệt vời! Bạn đã giải quyết mê cung trong {elapsedTime} giây!",
                $"Wow! Bạn đã đến đích thành công chỉ trong {elapsedTime} giây!",
                $"Chúc mừng! Bạn đã hoàn thành thử thách này trong {elapsedTime} giây!"
                    };

                    // Chọn ngẫu nhiên một thông báo
                    Random random = new Random();
                    int index = random.Next(messages.Length);

                    // Hiện thông báo chúc mừng
                    MessageBox.Show(messages[index], "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Dừng đồng hồ đếm
                    
                    return true; // Trả về true nếu robot đã di chuyển đến đích
                }

                return true; // Trả về true nếu robot đã di chuyển
            }
            return false; // Trả về false nếu robot không di chuyển
        }








        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                    MoveRobot(-1, 0);
                    return true;
                case Keys.Down:
                    MoveRobot(1, 0);
                    return true;
                case Keys.Left:
                    MoveRobot(0, -1);
                    return true;
                case Keys.Right:
                    MoveRobot(0, 1);
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }




        /// <summary>
        /// Vẽ lại lưới
        /// </summary>
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush brush;
            Rectangle rect;
            brush = new SolidBrush(Color.DarkGray);
            // Tô màu nền.
            rect = new Rectangle(10, 10, columns * squareSize + 1, rows * squareSize + 1);
            g.FillRectangle(brush, rect);
            brush.Dispose();
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                {
                    if (grid[r, c] == EMPTY)
                        brush = new SolidBrush(Color.White);
                    else if (grid[r, c] == ROBOT)
                        brush = new SolidBrush(Color.Red);
                    else if (grid[r, c] == TARGET)
                        brush = new SolidBrush(Color.Green);
                    else if (grid[r, c] == OBST)
                        brush = new SolidBrush(Color.Black);
                    else if (grid[r, c] == FRONTIER)
                        brush = new SolidBrush(Color.Blue);
                    else if (grid[r, c] == CLOSED)
                        brush = new SolidBrush(Color.Cyan);
                    else if (grid[r, c] == ROUTE)
                        brush = new SolidBrush(Color.Yellow);
                    rect = new Rectangle(11 + c * squareSize, 11 + r * squareSize, squareSize - 1, squareSize - 1);
                    g.FillRectangle(brush, rect);
                    brush.Dispose();
                }

            if (drawArrows.Checked)
            {
                // Vẽ tất cả các mũi tên từ mỗi trạng thái mở hoặc đóng
                // đến đỉnh trước của nó.
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < columns; c++)
                        // Nếu ô hiện tại là mục tiêu và đã tìm thấy giải pháp,
                        // hoặc thuộc đường đi đến mục tiêu,
                        // hoặc là trạng thái mở,
                        // hoặc là trạng thái đóng nhưng không phải vị trí ban đầu của robot
                        if ((grid[r, c] == TARGET && found) || grid[r, c] == ROUTE ||
                                grid[r, c] == FRONTIER || (grid[r, c] == CLOSED &&
                                !(r == robotStart.row && c == robotStart.col)))
                        {
                            // Đuôi của mũi tên là ô hiện tại, trong khi
                            // đầu mũi tên là ô đỉnh.
                            Cell head;
                            if (grid[r, c] == FRONTIER)
                                if (dijkstra.Checked)
                                    head = FindPrev(graph, new Cell(r, c));
                                else
                                    head = FindPrev(openSet, new Cell(r, c));
                            else
                                head = FindPrev(closedSet, new Cell(r, c));

                            // Tọa độ tâm của ô hiện tại
                            int tailX = 11 + c * squareSize + squareSize / 2;
                            int tailY = 11 + r * squareSize + squareSize / 2;
                            // Tọa độ tâm của ô đỉnh
                            int headX = 11 + head.col * squareSize + squareSize / 2;
                            int headY = 11 + head.row * squareSize + squareSize / 2;
                            int thickness = squareSize > 25 ? 2 : 1;

                            // Nếu ô hiện tại là mục tiêu
                            // hoặc thuộc đường đi đến mục tiêu ...
                            if (grid[r, c] == TARGET || grid[r, c] == ROUTE)
                            {
                                // ... vẽ một mũi tên màu đỏ chỉ đến mục tiêu.
                                DrawArrow(g, Color.Red, thickness, tailX, tailY, headX, headY);
                                // Else ...
                            }
                            else
                            {
                                // ... vẽ một mũi tên màu đen chỉ đến ô đỉnh.
                                DrawArrow(g, Color.Black, thickness, headX, headY, tailX, tailY);
                            }
                        }
            }

        } // end MainForm_Paint

        /// <summary>
        /// Vẽ mũi tên có độ dày và màu sắc chỉ định từ điểm (x2,y2) đến điểm (x1,y1)
        /// </summary>
        /// <param name="g">Đối tượng graphics</param>
        /// <param name="color">Màu của mũi tên</param>
        /// <param name="thickness">Độ dày của mũi tên</param>
        /// <param name="x1">Tọa độ x của điểm 1</param>
        /// <param name="y1">Tọa độ y của điểm 1</param>
        /// <param name="x2">Tọa độ x của điểm 2</param>
        /// <param name="y2">Tọa độ y của điểm 2</param>
        private void DrawArrow(Graphics g, Color color, int thickness, int x1, int y1, int x2, int y2)
        {
            // Tính ma trận biến đổi affine
            System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();

            double dx = x2 - x1, dy = y2 - y1;
            float angle = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
            int len = (int)Math.Sqrt(dx * dx + dy * dy);
            matrix.Translate(x1, y1);  // di chuyển đầu mũi tên đến điểm (x1,y1)
            matrix.Rotate(angle);      // xoay mũi tên 'angle' độ

            // Vẽ mũi tên ngang có độ dài 'len'
            // kết thúc tại điểm (0,0) với hai đầu mũi 'arrowSize' có độ dài
            // tạo thành góc 20 độ với trục của mũi tên ...
            System.Drawing.Drawing2D.GraphicsPath myPath = new System.Drawing.Drawing2D.GraphicsPath();

            myPath.AddLine(0, 0, len, 0);
            myPath.AddLine(0, 0, (int)(arrowSize * Math.Sin(70 * Math.PI / 180)), (int)(arrowSize * Math.Cos(70 * Math.PI / 180)));
            myPath.AddLine(0, 0, (int)(arrowSize * Math.Sin(70 * Math.PI / 180)), -(int)(arrowSize * Math.Cos(70 * Math.PI / 180)));
            myPath.Transform(matrix);

            Pen myPen = new Pen(color, thickness);
            g.DrawPath(myPen, myPath);
            // ... và biến đổi affine xử lý phần còn lại !!!!!!
        } // end DrawArrow

    }
}
