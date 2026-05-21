using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TPPreenchedor.Data.Models;
using TPPreenchedor.Services;

namespace TPPreenchedor.Forms
{
    public partial class UserManagementForm : Form
    {
        private readonly UsuarioService _usuarioService;
        private BindingList<Usuario> _usuarios;
        private DataGridView _grid;
        private TextBox _txtLogin;
        private TextBox _txtNome;
        private CheckBox _chkAtivo;
        private TextBox _txtNovaSenha;
        private Label _lblModo;
        private Button _btnSalvar;
        private Button _btnNovo;
        private Button _btnResetSenha;
        private Button _btnExcluir;
        private SplitContainer _split;
        private int? _usuarioSelecionadoId;
        private readonly int? _usuarioLogadoId;
        private bool _ignorarSelecao;

        public UserManagementForm()
            : this(null, null)
        {
        }

        public UserManagementForm(UsuarioService usuarioService, int? usuarioLogadoId = null)
        {
            _usuarioService = usuarioService ?? new UsuarioService();
            _usuarioLogadoId = usuarioLogadoId;
            InitializeComponent();
        }

        private bool IsInDesigner => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private void InitializeComponent()
        {
            BuildLayout();
        }

        private void BuildLayout()
        {
            Text = "Gerenciar usuarios";
            Icon = AppIconProvider.GetIcon();
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1080, 640);
            MinimumSize = new Size(860, 540);
            BackColor = Color.FromArgb(241, 244, 249);

            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(241, 244, 249),
                SplitterWidth = 10
            };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                ReadOnly = true,
                GridColor = Color.FromArgb(226, 231, 239),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
            };
            _grid.EnableHeadersVisualStyles = false;
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 233, 250);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            _grid.DefaultCellStyle.BackColor = Color.White;
            _grid.DefaultCellStyle.ForeColor = Color.FromArgb(42, 52, 68);
            _grid.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 252);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(33, 43, 60);
            _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 6, 6, 6);
            _grid.ColumnHeadersHeight = 38;
            _grid.RowTemplate.Height = 38;
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Login", HeaderText = "Login", FillWeight = 34 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NomeExibicao", HeaderText = "Nome", FillWeight = 36 });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "Ativo", HeaderText = "Ativo", FillWeight = 30 });
            _grid.SelectionChanged += Grid_SelectionChanged;

            var painelLista = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(18)
            };

            var layoutLista = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            layoutLista.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutLista.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutLista.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutLista.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var lblListaTitulo = new Label
            {
                Text = "Usuarios cadastrados",
                AutoSize = true,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 39, 58),
                Margin = new Padding(0, 0, 0, 4)
            };

            var lblListaSubtitulo = new Label
            {
                Text = "Selecione um registro para editar ou redefinir a senha.",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(105, 114, 128),
                Margin = new Padding(0, 0, 0, 14)
            };

            layoutLista.Controls.Add(lblListaTitulo, 0, 0);
            layoutLista.Controls.Add(lblListaSubtitulo, 0, 1);
            layoutLista.Controls.Add(_grid, 0, 2);
            painelLista.Controls.Add(layoutLista);

            _split.Panel1.Padding = new Padding(18, 18, 10, 18);
            _split.Panel1.Controls.Add(painelLista);

            var panelForm = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(22)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 11
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var lblTitulo = new Label
            {
                Text = "Cadastro de usuario",
                AutoSize = true,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 39, 58),
                Margin = new Padding(0, 0, 0, 4)
            };

            var lblDescricao = new Label
            {
                Text = "Crie novos acessos, atualize dados e gerencie senhas.",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(105, 114, 128),
                Margin = new Padding(0, 0, 0, 0)
            };

            _lblModo = new Label
            {
                Text = "Modo: novo usuario",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(43, 87, 154),
                Margin = new Padding(0, 8, 0, 0)
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
                Margin = new Padding(0, 0, 0, 6)
            };
            _txtLogin = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 12)
            };

            var lblNome = new Label
            {
                Text = "Nome de exibicao",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 6)
            };
            _txtNome = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 12)
            };

            _chkAtivo = new CheckBox
            {
                Text = "Usuario ativo",
                AutoSize = true,
                Checked = true,
                Margin = new Padding(0, 2, 0, 14)
            };

            var lblSenha = new Label
            {
                Text = "Senha para novo usuario / reset",
                AutoSize = true,
                Margin = new Padding(0, 2, 0, 6)
            };
            _txtNovaSenha = new TextBox
            {
                Dock = DockStyle.Fill,
                UseSystemPasswordChar = true,
                Margin = new Padding(0, 0, 0, 0)
            };

            var painelBotoes = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 2,
                Margin = new Padding(0, 18, 0, 0)
            };
            painelBotoes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            painelBotoes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            painelBotoes.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            painelBotoes.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

            _btnNovo = new Button
            {
                Text = "Novo",
                Dock = DockStyle.Fill,
                Height = 34,
                Margin = new Padding(0, 0, 8, 8)
            };
            _btnNovo.Click += BtnNovo_Click;

            _btnSalvar = new Button
            {
                Text = "Salvar",
                Dock = DockStyle.Fill,
                Height = 34,
                BackColor = Color.FromArgb(43, 87, 154),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8, 0, 0, 8)
            };
            _btnSalvar.FlatAppearance.BorderSize = 0;
            _btnSalvar.Click += BtnSalvar_Click;

            _btnResetSenha = new Button
            {
                Text = "Reset senha",
                Dock = DockStyle.Fill,
                Height = 34,
                Margin = new Padding(0, 0, 8, 0)
            };
            _btnResetSenha.Click += BtnResetSenha_Click;

            _btnExcluir = new Button
            {
                Text = "Excluir",
                Dock = DockStyle.Fill,
                Height = 34,
                BackColor = Color.FromArgb(250, 232, 232),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8, 0, 0, 0)
            };
            _btnExcluir.FlatAppearance.BorderColor = Color.FromArgb(220, 180, 180);
            _btnExcluir.Click += BtnExcluir_Click;

            painelBotoes.Controls.Add(_btnNovo, 0, 0);
            painelBotoes.Controls.Add(_btnSalvar, 1, 0);
            painelBotoes.Controls.Add(_btnResetSenha, 0, 1);
            painelBotoes.Controls.Add(_btnExcluir, 1, 1);

            layout.Controls.Add(lblTitulo, 0, 0);
            layout.Controls.Add(lblDescricao, 0, 1);
            layout.Controls.Add(_lblModo, 0, 2);
            layout.Controls.Add(lblLogin, 0, 3);
            layout.Controls.Add(_txtLogin, 0, 4);
            layout.Controls.Add(lblNome, 0, 5);
            layout.Controls.Add(_txtNome, 0, 6);
            layout.Controls.Add(_chkAtivo, 0, 7);
            layout.Controls.Add(lblSenha, 0, 8);
            layout.Controls.Add(_txtNovaSenha, 0, 9);
            layout.Controls.Add(painelBotoes, 0, 10);
            layout.Controls.Add(spacer, 0, 11);

            panelForm.Controls.Add(layout);

            _split.Panel2.Padding = new Padding(10, 18, 18, 18);
            _split.Panel2.Controls.Add(panelForm);

            Controls.Add(_split);
            Load += UserManagementForm_Load;
            Shown += UserManagementForm_Shown;
            Resize += UserManagementForm_Resize;
        }

        private void UserManagementForm_Load(object sender, EventArgs e)
        {
            if (IsInDesigner)
            {
                CarregarMockDesigner();
                LimparFormulario();
                return;
            }

            RecarregarUsuarios();
            LimparFormulario();
        }

        private void UserManagementForm_Shown(object sender, EventArgs e)
        {
            AjustarLayoutSplit();
        }

        private void UserManagementForm_Resize(object sender, EventArgs e)
        {
            AjustarLayoutSplit();
        }

        private void RecarregarUsuarios()
        {
            _ignorarSelecao = true;
            _usuarios = new BindingList<Usuario>(_usuarioService.ListarUsuarios());
            _grid.DataSource = _usuarios;
            BeginInvoke((Action)(() =>
            {
                _grid.ClearSelection();
                _grid.CurrentCell = null;
                _ignorarSelecao = false;
                AtualizarEstadoFormulario();
            }));
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (_ignorarSelecao)
            {
                return;
            }

            var usuario = _grid.CurrentRow?.DataBoundItem as Usuario;
            if (usuario == null)
            {
                AtualizarEstadoFormulario();
                return;
            }

            _usuarioSelecionadoId = usuario.Id;
            _txtLogin.Text = usuario.Login;
            _txtNome.Text = usuario.NomeExibicao;
            _chkAtivo.Checked = usuario.Ativo;
            _txtNovaSenha.Text = string.Empty;
            AtualizarEstadoFormulario();
        }

        private void BtnNovo_Click(object sender, EventArgs e)
        {
            LimparFormulario();
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (IsInDesigner)
            {
                return;
            }

            try
            {
                if (_usuarioSelecionadoId.HasValue)
                {
                    _usuarioService.AtualizarUsuario(_usuarioSelecionadoId.Value, _txtLogin.Text, _txtNome.Text, _chkAtivo.Checked);
                }
                else
                {
                    _usuarioService.CriarUsuario(_txtLogin.Text, _txtNome.Text, _txtNovaSenha.Text);
                }

                RecarregarUsuarios();
                SelecionarPorLogin(_txtLogin.Text.Trim());
                MessageBox.Show("Usuario salvo com sucesso.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnResetSenha_Click(object sender, EventArgs e)
        {
            if (IsInDesigner)
            {
                return;
            }

            if (!_usuarioSelecionadoId.HasValue)
            {
                MessageBox.Show("Selecione um usuario para redefinir a senha.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _usuarioService.RedefinirSenha(_usuarioSelecionadoId.Value, _txtNovaSenha.Text);
                _txtNovaSenha.Text = string.Empty;
                MessageBox.Show("Senha redefinida com sucesso.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnExcluir_Click(object sender, EventArgs e)
        {
            if (IsInDesigner)
            {
                return;
            }

            if (!_usuarioSelecionadoId.HasValue)
            {
                MessageBox.Show("Selecione um usuario para excluir.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacao = MessageBox.Show(
                "Deseja realmente excluir este usuario? Os itens vinculados a ele tambem serao removidos.",
                "Usuarios",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacao != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _usuarioService.ExcluirUsuario(_usuarioSelecionadoId.Value, _usuarioLogadoId);
                RecarregarUsuarios();
                LimparFormulario();
                MessageBox.Show("Usuario excluido com sucesso.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SelecionarPorLogin(string login)
        {
            var usuario = _usuarios.FirstOrDefault(x => x.Login == login);
            if (usuario == null)
            {
                return;
            }

            foreach (DataGridViewRow row in _grid.Rows)
            {
                var current = row.DataBoundItem as Usuario;
                if (current != null && current.Id == usuario.Id)
                {
                    _ignorarSelecao = true;
                    row.Selected = true;
                    _grid.CurrentCell = row.Cells[0];
                    _ignorarSelecao = false;
                    _usuarioSelecionadoId = usuario.Id;
                    AtualizarEstadoFormulario();
                    break;
                }
            }
        }

        private void LimparFormulario()
        {
            _ignorarSelecao = true;
            _usuarioSelecionadoId = null;
            _txtLogin.Text = string.Empty;
            _txtNome.Text = string.Empty;
            _chkAtivo.Checked = true;
            _txtNovaSenha.Text = string.Empty;
            _grid.ClearSelection();
            _grid.CurrentCell = null;
            _ignorarSelecao = false;
            AtualizarEstadoFormulario();
        }

        private void AtualizarEstadoFormulario()
        {
            var emEdicao = _usuarioSelecionadoId.HasValue;
            _lblModo.Text = emEdicao ? "Modo: editando usuario selecionado" : "Modo: novo usuario";
            _lblModo.ForeColor = emEdicao
                ? Color.FromArgb(130, 78, 25)
                : Color.FromArgb(43, 87, 154);

            _btnResetSenha.Enabled = emEdicao;
            _btnExcluir.Enabled = emEdicao;
        }

        private void AjustarLayoutSplit()
        {
            if (_split == null || _split.Width <= 0)
            {
                return;
            }

            _split.FixedPanel = FixedPanel.Panel2;
            _split.Panel1MinSize = 280;
            _split.Panel2MinSize = 320;

            var larguraDireita = 360;
            var larguraDisponivel = _split.Width - _split.SplitterWidth;
            var distancia = larguraDisponivel - larguraDireita;
            var minimo = _split.Panel1MinSize;
            var maximo = larguraDisponivel - _split.Panel2MinSize;

            if (maximo < minimo)
            {
                maximo = minimo;
            }

            if (distancia < minimo)
            {
                distancia = minimo;
            }

            if (distancia > maximo)
            {
                distancia = maximo;
            }

            if (distancia > 0)
            {
                _split.SplitterDistance = distancia;
            }
        }

        private void CarregarMockDesigner()
        {
            _ignorarSelecao = true;
            _usuarios = new BindingList<Usuario>
            {
                new Usuario { Id = 1, Login = "admin", NomeExibicao = "Administrador", Ativo = true },
                new Usuario { Id = 2, Login = "palladino.11", NomeExibicao = "Palladino", Ativo = true }
            };
            _grid.DataSource = _usuarios;
            _grid.ClearSelection();
            _grid.CurrentCell = null;
            _ignorarSelecao = false;
            AtualizarEstadoFormulario();
        }
    }
}
