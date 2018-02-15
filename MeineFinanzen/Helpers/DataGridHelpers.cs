// 02.10.2016 MeineFinanzen.Helpers DataGridHelper.cs
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
namespace MeineFinanzen.Helpers {    
    public static class DataGridHelper {
        // Gets the visual child of an element. The parent of the expected element    
        public static T GetVisualChild<T>(Visual parent) where T : Visual {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++) {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                    child = GetVisualChild<T>(v);
                if (child != null)
                    break;
            }
            return child;
        }
        // The row of the cell
        public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int column) {
            if (row != null) {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null) {
                    grid.ScrollIntoView(row, grid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(row);
                }
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }
        // The row index of the cell      
        public static DataGridCell GetCell(DataGrid grid, int row, int column) {
            grid.SelectedItem = grid.Items[row];
            grid.ScrollIntoView(grid.Items[row]);

            //grid.CurrentCell = grid.Items;
            DataGridRow rowContainer = grid.GetRow(row);
            return grid.GetCell(rowContainer, column);
        }
        public static DataGridRow GetRow(this DataGrid grid, int index) {
            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null) {
                grid.UpdateLayout();        // May be virtualized, bring into view and try again.
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }
        public static DataGridRow GetSelectedRow(this DataGrid grid) {
            return (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem);
        }
    }
}