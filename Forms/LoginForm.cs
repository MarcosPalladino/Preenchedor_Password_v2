using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TPPreenchedor.Data.Models;
using TPPreenchedor.Services;

namespace TPPreenchedor.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;
        private TextBox _txtLogin;
        private TextBox _txtSenha;
        private Label _lblMensagem;
        private Label _lblHint;

        public Usuario UsuarioAutenticado { get; private set; }

        public LoginForm()
            : this(null)
        {
        }

        public LoginForm(AuthService authService)
        {
            _authService = authService;
            InitializeComponent();
        }

        private bool IsInDesigner => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private void InitializeComponent()
        {
            BuildLayout();
        }

        private void BuildLayout()
        {
            Text = "Acesso";
            Icon = AppIconProvider.GetIcon();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(460, 360);
            BackColor = Color.WhiteSmoke;

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28),
                BackColor = Color.White
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 10,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

            var lblTitulo = new Label
            {
                Text = "TPPreenchedor",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 44, 74),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };

            var lblSubtitulo = new Label
            {
                Text = "Entre com seu usuario para carregar seus itens.",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };

            var spacer = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };

            var lblLogin = new Label
            {
                Text = "Login",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };

            _txtLogin = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };

            var lblSenha = new Label
            {
                Text = "Senha",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };

            _txtSenha = new TextBox
            {
                Dock = DockStyle.Fill,
                UseSystemPasswordChar = true,
                Margin = new Padding(0, 0, 0, 8)
            };

            var btnEntrar = new Button
            {
                Text = "Entrar",
                Width = 110,
                Height = 34,
                BackColor = Color.FromArgb(43, 87, 154),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0)
            };
            btnEntrar.FlatAppearance.BorderSize = 0;
            btnEntrar.Click += BtnEntrar_Click;

            var btnRestaurar = new Button
            {
                Text = "Restaurar admin",
                Width = 120,
                Height = 34,
                BackColor = Color.FromArgb(228, 235, 247),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnRestaurar.FlatAppearance.BorderColor = Color.FromArgb(196, 205, 223);
            btnRestaurar.Click += BtnRestaurar_Click;

            _lblMensagem = new Label
            {
                Text = string.Empty,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Firebrick,
                Margin = new Padding(0, 0, 0, 0)
            };

            _lblHint = new Label
            {
                Text = "Usuario inicial: admin | Senha inicial: admin123",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                ForeColor = Color.FromArgb(96, 96, 96),
                Margin = new Padding(0)
            };

            var painelBotoes = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Margin = new Padding(0, 4, 0, 0),
                Padding = new Padding(0)
            };
            painelBotoes.Controls.Add(btnEntrar);
            painelBotoes.Controls.Add(btnRestaurar);

            AcceptButton = btnEntrar;

            layout.Controls.Add(lblTitulo, 0, 0);
            layout.Controls.Add(lblSubtitulo, 0, 1);
            layout.Controls.Add(spacer, 0, 2);
            layout.Controls.Add(lblLogin, 0, 3);
            layout.Controls.Add(_txtLogin, 0, 4);
            layout.Controls.Add(lblSenha, 0, 5);
            layout.Controls.Add(_txtSenha, 0, 6);
            layout.Controls.Add(_lblMensagem, 0, 7);
            layout.Controls.Add(painelBotoes, 0, 8);
            layout.Controls.Add(_lblHint, 0, 9);

            panel.Controls.Add(layout);
            Controls.Add(panel);
        }

        private void BtnEntrar_Click(object sender, EventArgs e)
        {
            if (IsInDesigner || _authService == null)
            {
                return;
            }

            var usuario = _authService.Autenticar(_txtLogin.Text, _txtSenha.Text);
            if (usuario == null)
            {
                _lblMensagem.Text = "Login ou senha invalidos.";
                _lblMensagem.ForeColor = Color.Firebrick;
                return;
            }

            UsuarioAutenticado = usuario;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnRestaurar_Click(object sender, EventArgs e)
        {
            if (IsInDesigner)
            {
                _txtLogin.Text = "admin";
                _txtSenha.Text = "admin123";
                _lblMensagem.Text = "Acao indisponivel no Designer.";
                _lblMensagem.ForeColor = Color.FromArgb(130, 78, 25);
                return;
            }

            var confirmacao = MessageBox.Show(
                "Deseja restaurar o acesso do usuario admin com a senha padrao admin123?",
                "Acesso",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacao != DialogResult.Yes)
            {
                return;
            }

            Data.DatabaseBootstrapper.RestoreAdminAccess();
            _txtLogin.Text = "admin";
            _txtSenha.Text = "admin123";
            _lblMensagem.Text = "Acesso admin restaurado.";
            _lblMensagem.ForeColor = Color.FromArgb(50, 120, 70);
        }
    }
}
