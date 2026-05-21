using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TPPreenchedor.Data.Models;
using TPPreenchedor.Services;

namespace TPPreenchedor.Forms
{
    public partial class ChangePasswordForm : Form
    {
        private readonly Usuario _usuario;
        private readonly UsuarioService _usuarioService;
        private TextBox _txtSenhaAtual;
        private TextBox _txtNovaSenha;
        private TextBox _txtConfirmacao;

        public ChangePasswordForm()
            : this(new Usuario { Id = 0, Login = "designer" }, null)
        {
        }

        public ChangePasswordForm(Usuario usuario, UsuarioService usuarioService)
        {
            _usuario = usuario ?? new Usuario { Id = 0, Login = "usuario" };
            _usuarioService = usuarioService;
            InitializeComponent();
        }

        private bool IsInDesigner => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private void InitializeComponent()
        {
            BuildLayout();
        }

        private void BuildLayout()
        {
            Text = "Trocar senha";
            Icon = AppIconProvider.GetIcon();
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(420, 260);
            BackColor = Color.White;

            Controls.Add(new Label
            {
                Text = $"Usuario: {_usuario.Login}",
                AutoSize = true,
                Location = new Point(24, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            });

            Controls.Add(new Label { Text = "Senha atual", AutoSize = true, Location = new Point(24, 60) });
            _txtSenhaAtual = new TextBox { Left = 24, Top = 80, Width = 350, UseSystemPasswordChar = true };

            Controls.Add(new Label { Text = "Nova senha", AutoSize = true, Location = new Point(24, 116) });
            _txtNovaSenha = new TextBox { Left = 24, Top = 136, Width = 350, UseSystemPasswordChar = true };

            Controls.Add(new Label { Text = "Confirmacao", AutoSize = true, Location = new Point(24, 172) });
            _txtConfirmacao = new TextBox { Left = 24, Top = 192, Width = 350, UseSystemPasswordChar = true };

            var btnSalvar = new Button
            {
                Text = "Salvar",
                Width = 110,
                Height = 34,
                Left = 264,
                Top = 220,
                BackColor = Color.FromArgb(43, 87, 154),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.Click += BtnSalvar_Click;

            Controls.Add(_txtSenhaAtual);
            Controls.Add(_txtNovaSenha);
            Controls.Add(_txtConfirmacao);
            Controls.Add(btnSalvar);
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (IsInDesigner || _usuarioService == null)
            {
                return;
            }

            try
            {
                _usuarioService.AlterarSenhaAtual(_usuario.Id, _txtSenhaAtual.Text, _txtNovaSenha.Text, _txtConfirmacao.Text);
                MessageBox.Show("Senha atualizada com sucesso.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
