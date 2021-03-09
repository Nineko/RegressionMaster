using System;
using System.Collections.Generic;
using System.IO;

using Excel = Microsoft.Office.Interop.Excel;


namespace RegressionMaster
{
    class ExcelExplor
    {
        public const int MAX_ROW = 1048576;
        public const int MAX_COLUMN = 16384;

        private readonly string _filePath;
        private string _fileName => $"{_filePath}\\{DateTime.Today:yyyy-MM-dd}.xlsx";
        private string _tmpFileName => _fileName + ".tmp";

        private bool deleteTemporaryFile;
        private int currentTransactionRow = 1;

        private Excel.Application mergeInstanceApplication;
        private Excel.Application excelApplication;
        private Excel.Workbook excelWorkbook;
        private Excel.Worksheet excelWorksheet;

        public ExcelExplor(string filePath)
        {
            _filePath = filePath;
        }

        public void Close()
        {
            excelWorksheet = null;

            excelWorkbook.Close();
            excelWorkbook = null;

            excelApplication.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApplication);
            excelApplication = null;

            if (mergeInstanceApplication.Workbooks.Count == 0)
            {
                mergeInstanceApplication.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(mergeInstanceApplication);
                mergeInstanceApplication = null;
            }

            if (deleteTemporaryFile)
                File.Delete(this._tmpFileName);
        }

        public void Initialize()
        {
            mergeInstanceApplication = new Excel.Application(); // Create a new application for another excel to merge instance.
            excelApplication = new Excel.Application();

            if (!File.Exists(this._fileName))
                excelWorkbook = excelApplication.Workbooks.Add();

            if (File.Exists(this._fileName))
            {
                File.Copy(this._fileName, this._tmpFileName, true);
                excelWorkbook = excelApplication.Application.Workbooks.Open(_tmpFileName, ReadOnly: false);
            }

            excelApplication.DisplayAlerts = false;

            excelWorkbook = excelApplication.Workbooks[1];
            excelWorkbook.Activate();

            int worksheetNum = excelWorkbook.Worksheets.Count;
            excelWorksheet = excelWorkbook.Worksheets[worksheetNum];
            excelWorksheet.Activate();

        }

        private void AddExcelWorkSheet()
        {
            excelWorksheet = excelWorkbook.Worksheets.Add(After: excelWorksheet);
            currentTransactionRow = 1;
        }

    }
}

