using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using TPPreenchedor.Data;
using TPPreenchedor.Data.Models;
using TPPreenchedor.Services;

namespace TPPreenchedor.Forms
{
    public partial class Preenchedor : Form
    {
        private struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        private struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        private struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        private const int INPUT_KEYBOARD = 1;
        private const int KEYEVENTF_UNICODE = 4;
        private const int KEYEVENTF_KEYUP = 2;

        private readonly Usuario _usuario;
        private readonly ItemPreenchimentoRepository _itemRepository;
        private readonly UsuarioService _usuarioService;
        private TrackBar _trackBar;
        private Label _lblTempo;
        private DataGridView _gridCredenciais;
        private BindingList<ItemPreenchimento> _credenciais;
        private FlowLayoutPanel _painelTextos;

        public Preenchedor()
            : this(new Usuario { Id = 0, Login = "designer", NomeExibicao = "Designer" })
        {
        }

        public Preenchedor(Usuario usuario)
        {
            _usuario = usuario ?? new Usuario { Id = 0, Login = "usuario", NomeExibicao = "Usuario" };
            _itemRepository = new ItemPreenchimentoRepository();
            _usuarioService = new UsuarioService();
            InitializeComponent();
        }

        private bool IsInDesigner => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private void InitializeComponent()
        {
            BuildLayout();
        }

        private void BuildLayout()
        {
            Text = "TPPreenchedor";
            Icon = AppIconProvider.GetIcon();
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(980, 680);
            BackColor = Color.FromArgb(243, 246, 251);

            var topo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 92,
                BackColor = Color.White
            };

            var lblTitulo = new Label
            {
                Text = "TPPreenchedor",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 44, 74),
                AutoSize = true,
                Location = new Point(22, 14)
            };

            var lblUsuario = new Label
            {
                Text = $"Usuario: {_usuario.NomeExibicao} ({_usuario.Login})",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(24, 50)
            };

            _lblTempo = new Label
            {
                Text = "Tempo de espera (3 seg)",
                AutoSize = true,
                Location = new Point(610, 18),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 67, 98)
            };

            _trackBar = new TrackBar
            {
                Minimum = 3,
                Maximum = 20,
                Value = 3,
                TickStyle = TickStyle.None,
                Width = 260,
                Location = new Point(610, 42)
            };
            _trackBar.ValueChanged += TrackBar_ValueChanged;

            var btnUsuarios = new Button
            {
                Text = "Usuarios",
                Width = 100,
                Height = 34,
                Location = new Point(472, 34),
                BackColor = Color.FromArgb(228, 235, 247),
                FlatStyle = FlatStyle.Flat
            };
            btnUsuarios.FlatAppearance.BorderColor = Color.FromArgb(196, 205, 223);
            btnUsuarios.Click += BtnUsuarios_Click;

            var btnTrocarSenha = new Button
            {
                Text = "Trocar senha",
                Width = 120,
                Height = 34,
                Location = new Point(344, 34),
                BackColor = Color.FromArgb(228, 235, 247),
                FlatStyle = FlatStyle.Flat
            };
            btnTrocarSenha.FlatAppearance.BorderColor = Color.FromArgb(196, 205, 223);
            btnTrocarSenha.Click += BtnTrocarSenha_Click;

            topo.Controls.Add(lblTitulo);
            topo.Controls.Add(lblUsuario);
            topo.Controls.Add(_lblTempo);
            topo.Controls.Add(_trackBar);
            topo.Controls.Add(btnTrocarSenha);
            topo.Controls.Add(btnUsuarios);

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(18, 8),
                Font = new Font("Segoe UI", 10F)
            };

            var tabCredenciais = new TabPage("Credenciais") { BackColor = Color.FromArgb(243, 246, 251) };
            var tabTextos = new TabPage("Textos") { BackColor = Color.FromArgb(243, 246, 251) };

            tabCredenciais.Controls.Add(BuildCredenciaisTab());
            tabTextos.Controls.Add(BuildTextosTab());

            tabs.TabPages.Add(tabCredenciais);
            tabs.TabPages.Add(tabTextos);

            Controls.Add(tabs);
            Controls.Add(topo);

            Load += Preenchedor_Load;
        }

        private Control BuildCredenciaisTab()
        {
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18)
            };

            var acoes = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52
            };

            var btnAdicionar = new Button
            {
                Text = "Nova credencial",
                Width = 140,
                Height = 34,
                BackColor = Color.FromArgb(43, 87, 154),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(0, 8)
            };
            btnAdicionar.FlatAppearance.BorderSize = 0;
            btnAdicionar.Click += BtnAdicionarCredencial_Click;

            var lblHint = new Label
            {
                Text = "Edite diretamente na grade. O salvamento acontece ao sair da celula.",
                AutoSize = true,
                Location = new Point(160, 15),
                ForeColor = Color.DimGray
            };

            acoes.Controls.Add(btnAdicionar);
            acoes.Controls.Add(lblHint);

            _gridCredenciais = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _gridCredenciais.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 233, 250);
            _gridCredenciais.DefaultCellStyle.SelectionForeColor = Color.Black;
            _gridCredenciais.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _gridCredenciais.CellContentClick += GridCredenciais_CellContentClick;
            _gridCredenciais.CellEndEdit += GridCredenciais_CellEndEdit;

            _gridCredenciais.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TituloExibicao",
                HeaderText = "Titulo",
                FillWeight = 28
            });

            _gridCredenciais.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoginReferencia",
                HeaderText = "Login/Referencia",
                FillWeight = 24
            });

            _gridCredenciais.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Valor",
                HeaderText = "Valor",
                FillWeight = 32
            });

            _gridCredenciais.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Preencher",
                HeaderText = "Acao",
                Text = "Preencher",
                UseColumnTextForButtonValue = true,
                FillWeight = 12
            });

            _gridCredenciais.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Excluir",
                HeaderText = "",
                Text = "Excluir",
                UseColumnTextForButtonValue = true,
                FillWeight = 10
            });

            container.Controls.Add(_gridCredenciais);
            container.Controls.Add(acoes);

            return container;
        }

        private Control BuildTextosTab()
        {
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18)
            };

            var topo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52
            };

            var btnAdicionar = new Button
            {
                Text = "Novo texto",
                Width = 120,
                Height = 34,
                BackColor = Color.FromArgb(43, 87, 154),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(0, 8)
            };
            btnAdicionar.FlatAppearance.BorderSize = 0;
            btnAdicionar.Click += BtnAdicionarTexto_Click;

            var lblHint = new Label
            {
                Text = "Use os campos abaixo para textos maiores e acione o preenchimento por bloco.",
                AutoSize = true,
                Location = new Point(140, 15),
                ForeColor = Color.DimGray
            };

            topo.Controls.Add(btnAdicionar);
            topo.Controls.Add(lblHint);

            _painelTextos = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 12)
            };

            container.Controls.Add(_painelTextos);
            container.Controls.Add(topo);

            return container;
        }

        private void Preenchedor_Load(object sender, EventArgs e)
        {
            if (IsInDesigner)
            {
                CarregarMockDesigner();
                return;
            }

            RecarregarDados();
        }

        private void RecarregarDados()
        {
            var credenciais = _itemRepository.ObterItensPorUsuario(_usuario.Id, TipoItemPreenchimento.TextoCurto);
            _credenciais = new BindingList<ItemPreenchimento>(credenciais);
            _gridCredenciais.DataSource = _credenciais;

            var textos = _itemRepository.ObterItensPorUsuario(_usuario.Id, TipoItemPreenchimento.TextoLongo);
            RenderizarTextos(textos);
        }

        private void RenderizarTextos(System.Collections.Generic.List<ItemPreenchimento> textos)
        {
            _painelTextos.SuspendLayout();
            _painelTextos.Controls.Clear();

            foreach (var item in textos)
            {
                var card = new Panel
                {
                    Width = Math.Max(_painelTextos.ClientSize.Width - 30, 780),
                    Height = 220,
                    BackColor = Color.White,
                    Margin = new Padding(0, 0, 0, 14),
                    Padding = new Padding(16)
                };

                var txtTitulo = new TextBox
                {
                    Text = item.TituloExibicao,
                    Width = card.Width - 32,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = item
                };

                var txtReferencia = new TextBox
                {
                    Text = item.LoginReferencia ?? string.Empty,
                    Width = card.Width - 32,
                    Top = 38,
                    Font = new Font("Segoe UI", 9F),
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = item
                };

                var txtValor = new TextBox
                {
                    Text = item.Valor ?? string.Empty,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    Width = card.Width - 32,
                    Height = 100,
                    Top = 72,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 9.5F),
                    Tag = item
                };

                var btnSalvar = new Button
                {
                    Text = "Salvar",
                    Width = 96,
                    Height = 32,
                    Top = 178,
                    Left = card.Width - 216,
                    BackColor = Color.FromArgb(228, 235, 247),
                    FlatStyle = FlatStyle.Flat,
                    Tag = new TextCardState(item, txtTitulo, txtReferencia, txtValor)
                };
                btnSalvar.FlatAppearance.BorderColor = Color.FromArgb(196, 205, 223);
                btnSalvar.Click += BtnSalvarTexto_Click;

                var btnPreencher = new Button
                {
                    Text = "Preencher",
                    Width = 100,
                    Height = 32,
                    Top = 178,
                    Left = card.Width - 108,
                    BackColor = Color.FromArgb(43, 87, 154),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Tag = new TextCardState(item, txtTitulo, txtReferencia, txtValor)
                };
                btnPreencher.FlatAppearance.BorderSize = 0;
                btnPreencher.Click += BtnPreencherTexto_Click;

                var btnExcluir = new Button
                {
                    Text = "Excluir",
                    Width = 90,
                    Height = 32,
                    Top = 178,
                    Left = card.Width - 314,
                    BackColor = Color.FromArgb(250, 232, 232),
                    FlatStyle = FlatStyle.Flat,
                    Tag = item
                };
                btnExcluir.FlatAppearance.BorderColor = Color.FromArgb(220, 180, 180);
                btnExcluir.Click += BtnExcluirTexto_Click;

                card.Controls.Add(txtTitulo);
                card.Controls.Add(txtReferencia);
                card.Controls.Add(txtValor);
                card.Controls.Add(btnExcluir);
                card.Controls.Add(btnSalvar);
                card.Controls.Add(btnPreencher);

                _painelTextos.Controls.Add(card);
            }

            _painelTextos.ResumeLayout();
        }

        private void TrackBar_ValueChanged(object sender, EventArgs e)
        {
            _lblTempo.Text = $"Tempo de espera ({_trackBar.Value} seg)";
        }

        private void BtnTrocarSenha_Click(object sender, EventArgs e)
        {
            if (IsInDesigner)
            {
                return;
            }

            using (var form = new ChangePasswordForm(_usuario, _usuarioService))
            {
                form.ShowDialog(this);
            }
        }

        private void BtnUsuarios_Click(object sender, EventArgs e)
        {
            if (IsInDesigner)
            {
                return;
            }

            using (var form = new UserManagementForm(_usuarioService, _usuario.Id))
            {
                form.ShowDialog(this);
            }
        }

        private void BtnAdicionarCredencial_Click(object sender, EventArgs e)
        {
            var novoItem = new ItemPreenchimento
            {
                UsuarioId = _usuario.Id,
                TituloExibicao = "Nova credencial",
                LoginReferencia = string.Empty,
                TipoItem = TipoItemPreenchimento.TextoCurto,
                Valor = string.Empty,
                OrdemExibicao = _itemRepository.ObterProximaOrdem(_usuario.Id, TipoItemPreenchimento.TextoCurto),
                Ativo = true
            };

            _itemRepository.Adicionar(novoItem);
            RecarregarDados();
        }

        private void BtnAdicionarTexto_Click(object sender, EventArgs e)
        {
            var novoItem = new ItemPreenchimento
            {
                UsuarioId = _usuario.Id,
                TituloExibicao = "Novo texto",
                LoginReferencia = string.Empty,
                TipoItem = TipoItemPreenchimento.TextoLongo,
                Valor = string.Empty,
                OrdemExibicao = _itemRepository.ObterProximaOrdem(_usuario.Id, TipoItemPreenchimento.TextoLongo),
                Ativo = true
            };

            _itemRepository.Adicionar(novoItem);
            RecarregarDados();
        }

        private void GridCredenciais_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            var item = _gridCredenciais.Rows[e.RowIndex].DataBoundItem as ItemPreenchimento;
            if (item == null)
            {
                return;
            }

            _itemRepository.Atualizar(item);
        }

        private async void GridCredenciais_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            var item = _gridCredenciais.Rows[e.RowIndex].DataBoundItem as ItemPreenchimento;
            if (item == null)
            {
                return;
            }

            var columnName = _gridCredenciais.Columns[e.ColumnIndex].Name;
            if (columnName == "Excluir")
            {
                ExcluirItemCurto(item);
                return;
            }

            if (columnName != "Preencher")
            {
                return;
            }

            _itemRepository.Atualizar(item);
            await PreencherValorAsync(item.Valor, item.TituloExibicao);
        }

        private void BtnSalvarTexto_Click(object sender, EventArgs e)
        {
            var state = ((Button)sender).Tag as TextCardState;
            if (state == null)
            {
                return;
            }

            state.Item.TituloExibicao = state.Titulo.Text.Trim();
            state.Item.LoginReferencia = state.Referencia.Text.Trim();
            state.Item.Valor = state.Valor.Text;
            _itemRepository.Atualizar(state.Item);
            MessageBox.Show("Texto salvo.", "TPPreenchedor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BtnPreencherTexto_Click(object sender, EventArgs e)
        {
            var state = ((Button)sender).Tag as TextCardState;
            if (state == null)
            {
                return;
            }

            state.Item.TituloExibicao = state.Titulo.Text.Trim();
            state.Item.LoginReferencia = state.Referencia.Text.Trim();
            state.Item.Valor = state.Valor.Text;
            _itemRepository.Atualizar(state.Item);

            await PreencherValorAsync(state.Item.Valor, state.Item.TituloExibicao);
        }

        private void BtnExcluirTexto_Click(object sender, EventArgs e)
        {
            var item = ((Button)sender).Tag as ItemPreenchimento;
            if (item == null)
            {
                return;
            }

            var confirmacao = MessageBox.Show(
                $"Deseja excluir o texto '{item.TituloExibicao}'?",
                "TPPreenchedor",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacao != DialogResult.Yes)
            {
                return;
            }

            _itemRepository.Remover(item.Id);
            RecarregarDados();
        }

        private void ExcluirItemCurto(ItemPreenchimento item)
        {
            var confirmacao = MessageBox.Show(
                $"Deseja excluir a credencial '{item.TituloExibicao}'?",
                "TPPreenchedor",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacao != DialogResult.Yes)
            {
                return;
            }

            _itemRepository.Remover(item.Id);
            RecarregarDados();
        }

        private async Task PreencherValorAsync(string valor, string titulo)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                MessageBox.Show(
                    $"Voce precisa informar os dados para preencher '{titulo}'.",
                    "TPPreenchedor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ToggleControls(false);

            try
            {
                await Task.Delay(_trackBar.Value * 1000);

                foreach (var caractere in valor)
                {
                    SendUnicodeChar(caractere);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocorreu um erro inesperado: " + ex.Message,
                    "TPPreenchedor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                ToggleControls(true);
            }
        }

        private void ToggleControls(bool enabled)
        {
            _trackBar.Enabled = enabled;
            _gridCredenciais.Enabled = enabled;
            _painelTextos.Enabled = enabled;
        }

        private void SendUnicodeChar(char c)
        {
            var input = new INPUT[1];
            input[0].type = INPUT_KEYBOARD;
            input[0].u.ki.wVk = 0;
            input[0].u.ki.wScan = (short)c;
            input[0].u.ki.dwFlags = KEYEVENTF_UNICODE;
            input[0].u.ki.time = 0;
            input[0].u.ki.dwExtraInfo = IntPtr.Zero;

            if (SendInput(1u, input, Marshal.SizeOf(input[0])) == 0)
            {
                throw new Exception("Erro ao enviar entrada de teclado.");
            }

            input[0].u.ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;
            if (SendInput(1u, input, Marshal.SizeOf(input[0])) == 0)
            {
                throw new Exception("Erro ao enviar entrada de teclado.");
            }
        }

        private class TextCardState
        {
            public TextCardState(ItemPreenchimento item, TextBox titulo, TextBox referencia, TextBox valor)
            {
                Item = item;
                Titulo = titulo;
                Referencia = referencia;
                Valor = valor;
            }

            public ItemPreenchimento Item { get; }
            public TextBox Titulo { get; }
            public TextBox Referencia { get; }
            public TextBox Valor { get; }
        }

        private void CarregarMockDesigner()
        {
            _credenciais = new BindingList<ItemPreenchimento>
            {
                new ItemPreenchimento
                {
                    Id = 1,
                    TituloExibicao = "PWD USUARIO TPB",
                    LoginReferencia = @"tpb\\palladino.11",
                    Valor = "******",
                    TipoItem = TipoItemPreenchimento.TextoCurto,
                    OrdemExibicao = 1,
                    Ativo = true
                },
                new ItemPreenchimento
                {
                    Id = 2,
                    TituloExibicao = "PWD USUARIO ITAU",
                    LoginReferencia = @"tpitau\\palladino.11",
                    Valor = "******",
                    TipoItem = TipoItemPreenchimento.TextoCurto,
                    OrdemExibicao = 2,
                    Ativo = true
                }
            };
            _gridCredenciais.DataSource = _credenciais;

            RenderizarTextos(new System.Collections.Generic.List<ItemPreenchimento>
            {
                new ItemPreenchimento
                {
                    Id = 3,
                    TituloExibicao = "Texto padrao",
                    LoginReferencia = "Uso livre",
                    Valor = "Exemplo de texto longo para visualizacao no Designer.",
                    TipoItem = TipoItemPreenchimento.TextoLongo,
                    OrdemExibicao = 1,
                    Ativo = true
                }
            });
        }
    }
}
