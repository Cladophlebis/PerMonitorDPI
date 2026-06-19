using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WpfApp.Utils
{
    /// <summary>
    /// ウィンドウ座標の取得/設定のためのパラメータ群を格納するクラス
    /// </summary>
    public class WindowSettings
    {
        /// <summary>
        /// ウィンドウ位置(Left)(物理座標)
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// ウィンドウ位置(Top)(物理座標)
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// ウィンドウサイズ(Width)(論理サイズ)
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// ウィンドウサイズ(Height)(論理サイズ)
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// ウィンドウの表示状態
        /// </summary>
        public WindowState State { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WindowSettings()
        {
            State = WindowState.Normal;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="state">ウィンドウの表示状態(デフォルト:Normal)</param>
        public WindowSettings(int left, int top, double width, double height, WindowState state = WindowState.Normal)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            State = state;
        }
    }

    public class WindowSettingsHelper
    {
        #region Win32 API Definitions

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT lprc, uint dwFlags);

        // フラグ定義：どこのモニターにも引っかからなかったら一番近いモニターを返す
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
        // フラグ定義：どこのモニターにも引っかからなかったら null (IntPtr.Zero) を返す
        private const uint MONITOR_DEFAULTTONULL = 0x00000000;
        
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwpl);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // 各種フラグの定義
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private const uint SWP_NOSIZE = 0x0001; // サイズを変更しない場合
        
        // ウィンドウ位置・状態を保持するWin32構造体
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public int length;  //sizeof(WINDOWPLACEMENT)を格納しておく
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition; // ← DPIに依存しない物理座標
            //public RECT rcDevice;
        }

        private static WINDOWPLACEMENT CreateWindowPlacement()
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf<WINDOWPLACEMENT>();
            placement.flags = 0;
            return placement;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x; public int y; }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int left; public int top; public int right; public int bottom; }

        #endregion

        /// <summary>
        /// 設定に従ってウィンドウ位置・状態を復元する。
        /// </summary>
        /// <param name="win">対象のウィンドウ</param>
        /// <param name="ws">ウィンドウ位置・状態</param>
        public void Set(Window win, WindowSettings ws)
        {
            SetPos(win, ws);
            SetSize(win, ws);
        }


        /// <summary>
        /// 設定に従ってウィンドウ位置を復元する。
        /// <para>ウィンドウサイズはいじらず、位置だけを復元する。</para>
        /// </summary>
        /// <param name="win">対象のウィンドウ</param>
        /// <param name="ws">ウィンドウ位置・状態</param>
        public void SetPos(Window win, WindowSettings ws)
        {
            IntPtr hwnd = new WindowInteropHelper(win).Handle;

            // 現在のモニター群の「どこか」に引っかかるかチェック
            POINT ptCenter = new POINT();
            ptCenter.x = ws.Left + (int)(ws.Width / 2.0);
            ptCenter.y = ws.Top + (int)(ws.Height / 2.0);
            IntPtr hMonitor = MonitorFromPoint(ptCenter, MONITOR_DEFAULTTONULL);
            if (hMonitor == IntPtr.Zero)
            {
                // 中心が画面外の場合はデフォルト位置のまま
                // (dpi100%でない場合は中心にはならないので不正確だが画面外判定には十分)
            }
            else
            {
                //　位置を復元
                if (!SetWindowPos(hwnd, HWND_TOP, ws.Left, ws.Top, 0, 0, SWP_NOSIZE))
                {
                    throw new Win32Exception();
                }
            }
        }


        /// <summary>
        /// 設定に従ってウィンドウサイズ・状態を復元する。
        /// <para>ウィンドウ位置はいじらず、サイズ・状態だけを復元する。</para>
        /// </summary>
        /// <param name="win">対象のウィンドウ</param>
        /// <param name="ws">ウィンドウ位置・状態</param>
        public void SetSize(Window win, WindowSettings ws)
        {
            // サイズを指定
            win.WindowState = WindowState.Normal;   // 通常状態のサイズとして指定
            win.Width = ws.Width;
            win.Height = ws.Height;
            // 最大化/最小化の設定
            // 最小化状態で保存されていた場合は、次回起動時は「通常サイズ」で開くのが親切
            //win.WindowState = ws.State;
            win.WindowState = (ws.State == WindowState.Minimized) ? WindowState.Normal : ws.State;
        }


        /// <summary>
        /// ウィンドウ位置・状態を返す
        /// </summary>
        /// <param name="win">対象のウィンドウ</param>
        /// <returns>ウィンドウ位置・状態</returns>
        /// <exception cref="Win32Exception"></exception>
        public WindowSettings Get(Window win)
        {
            IntPtr hwnd = new WindowInteropHelper(win).Handle;

            WINDOWPLACEMENT placement = CreateWindowPlacement();

            // 現在の正確なネイティブ座標と状態を取得
            if (!GetWindowPlacement(hwnd, ref placement))
            {
                throw new Win32Exception();
            }
            var ws = new WindowSettings();
            ws.Left = placement.rcNormalPosition.left;
            ws.Top = placement.rcNormalPosition.top;
            ws.State = win.WindowState;
            // サイズは論理サイズで保存する。
            ws.Width = win.RestoreBounds.Width;
            ws.Height = win.RestoreBounds.Height;
            return ws;
        }

    }
}
