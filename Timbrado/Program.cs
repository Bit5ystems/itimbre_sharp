using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EjemploTimbrado
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmPrincipal());
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(
                string.Format("Error: {0}", e.Exception.Message),
                "iTimbre - Ejemplo de Conexión",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
                );
        }
    }
}
