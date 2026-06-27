using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Repositories;
using Models;

namespace UI
{
    public class FormCadastro : Form
    {
        // Menu lateral
        private Panel pnlMenu = new Panel();
        private Button btnDashboard = new Button();
        private Button btnCadastrar = new Button();
        private Button btnListar = new Button();
        private Button btnEditar = new Button();
        private Button btnApagar = new Button();

        // Área principal
        private Panel pnlMain = new Panel();

        // Painel de cadastro (central)
        private Panel pnlCadastro = new Panel();
        private Label lblNome = new Label();
        private TextBox txtNome = new TextBox();
        private Label lblEmail = new Label();
        private TextBox txtEmail = new TextBox();
        private Label lblTelefone = new Label();
        private TextBox txtTelefone = new TextBox();
        private Button btnSalvar = new Button();

        // DataGridView profissional para listagem
        private DataGridView dgvClientes = new DataGridView();
        private TextBox txtFiltro = new TextBox();
        private Label lblFiltro = new Label();
        private Button btnRefresh = new Button();

        // Dashboard cards
        private Panel cardCadastrados = new Panel();
        private Label cardCadastradosTitle = new Label();
        private Label cardCadastradosValue = new Label();
        private Label cardCadastradosSubtitle = new Label();

        private Panel cardEditados = new Panel();
        private Label cardEditadosTitle = new Label();
        private Label cardEditadosValue = new Label();
        private Label cardEditadosSubtitle = new Label();

        private Panel cardDeletados = new Panel();
        private Label cardDeletadosTitle = new Label();
        private Label cardDeletadosValue = new Label();
        private Label cardDeletadosSubtitle = new Label();

        private ClienteRepository repo = new ClienteRepository();

        // Edição
        private bool emEdicao = false;
        private int editarId = 0;

        public FormCadastro()
        {
            InitializeForm();
            MontarMenu();
            MontarAreaPrincipal();

            // Reposiciona controles quando o painel principal mudar de tamanho
            pnlMain.Resize += (s, e) =>
            {
                PositionCadastroControls();
                PositionGridControls();
                PositionDashboardCards();
            };

            // inicia na dashboard
            MostrarDashboard();
        }

        private void InitializeForm()
        {
            this.Text = "Sistema de Clientes";
            this.Size = new Size(980, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.BackColor = Color.WhiteSmoke;
        }

        private void MontarMenu()
        {
            pnlMenu.Dock = DockStyle.Left;
            pnlMenu.Width = 160;
            pnlMenu.BackColor = Color.FromArgb(245, 245, 245);

            btnDashboard.Text = "Dashboard";
            btnDashboard.SetBounds(10, 20, 140, 40);
            btnDashboard.Click += (s, e) => MostrarDashboard();

            btnCadastrar.Text = "Cadastrar";
            btnCadastrar.SetBounds(10, 70, 140, 40);
            btnCadastrar.Click += (s, e) => MostrarCadastro();

            btnListar.Text = "Listar";
            btnListar.SetBounds(10, 120, 140, 40);
            btnListar.Click += (s, e) => MostrarLista();

            btnEditar.Text = "Editar";
            btnEditar.SetBounds(10, 170, 140, 40);
            btnEditar.Click += (s, e) => IniciarEdicaoSelecionada();

            btnApagar.Text = "Apagar";
            btnApagar.SetBounds(10, 220, 140, 40);
            btnApagar.Click += (s, e) => ApagarSelecionado();

            pnlMenu.Controls.AddRange(new Control[] { btnDashboard, btnCadastrar, btnListar, btnEditar, btnApagar });
            this.Controls.Add(pnlMenu);

            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Padding = new Padding(12);
            this.Controls.Add(pnlMain);
        }

        private void MontarAreaPrincipal()
        {
            // --- Painel de Cadastro (centralizável) ---
            pnlCadastro.Size = new Size(700, 220);
            pnlCadastro.BackColor = Color.Transparent;
            pnlCadastro.Anchor = AnchorStyles.None;
            pnlMain.Controls.Add(pnlCadastro);

            // Labels e TextBoxes dentro do painel de cadastro
            lblNome.Text = "Nome:";
            lblNome.AutoSize = true;
            lblNome.Location = new Point(0, 6);

            txtNome.Size = new Size(640, 28);
            txtNome.Location = new Point(0, 28);
            ConfigureTextBox(txtNome);

            lblEmail.Text = "Email:";
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(0, 68);

            txtEmail.Size = new Size(360, 28);
            txtEmail.Location = new Point(0, 90);
            ConfigureTextBox(txtEmail);

            lblTelefone.Text = "Telefone:";
            lblTelefone.AutoSize = true;
            lblTelefone.Location = new Point(380, 68);

            txtTelefone.Size = new Size(260, 28);
            txtTelefone.Location = new Point(380, 90);
            ConfigureTextBox(txtTelefone);

            btnSalvar.Text = "Salvar";
            btnSalvar.Size = new Size(110, 36);
            btnSalvar.Location = new Point(0, 140);
            btnSalvar.Click += BtnSalvar_Click;

            pnlCadastro.Controls.AddRange(new Control[] {
                lblNome, txtNome,
                lblEmail, txtEmail,
                lblTelefone, txtTelefone,
                btnSalvar
            });

            // --- DataGridView (Listagem profissional) ---
            dgvClientes.AllowUserToAddRows = false;
            dgvClientes.AllowUserToDeleteRows = false;
            dgvClientes.ReadOnly = true;
            dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClientes.MultiSelect = false;
            dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvClientes.RowHeadersVisible = false;
            dgvClientes.BackgroundColor = Color.White;
            dgvClientes.BorderStyle = BorderStyle.FixedSingle;
            dgvClientes.EnableHeadersVisualStyles = false;
            dgvClientes.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 144, 255);
            dgvClientes.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvClientes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvClientes.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvClientes.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            dgvClientes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 153, 255);
            dgvClientes.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvClientes.AutoGenerateColumns = false;
            dgvClientes.Visible = false;
            dgvClientes.DoubleClick += (s, e) => IniciarEdicaoSelecionada();
            dgvClientes.CellContentClick += DgvClientes_CellContentClick;

            // Colunas de dados
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", FillWeight = 10 });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nome", HeaderText = "Nome", DataPropertyName = "Nome", FillWeight = 40 });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", DataPropertyName = "Email", FillWeight = 30 });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Telefone", HeaderText = "Telefone", DataPropertyName = "Telefone", FillWeight = 20 });

            // Coluna de ação: Editar (botão)
            var colEditar = new DataGridViewButtonColumn
            {
                Name = "EditarBtn",
                HeaderText = "",
                Text = "Editar",
                UseColumnTextForButtonValue = true,
                FillWeight = 12,
                FlatStyle = FlatStyle.Standard
            };
            dgvClientes.Columns.Add(colEditar);

            // Coluna de ação: Apagar (botão)
            var colApagar = new DataGridViewButtonColumn
            {
                Name = "ApagarBtn",
                HeaderText = "",
                Text = "Apagar",
                UseColumnTextForButtonValue = true,
                FillWeight = 12,
                FlatStyle = FlatStyle.Standard
            };
            dgvClientes.Columns.Add(colApagar);

            // Filtro e refresh
            lblFiltro.Text = "Pesquisar:";
            lblFiltro.AutoSize = true;
            lblFiltro.Visible = false;

            txtFiltro.Width = 300;
            txtFiltro.Visible = false;
            txtFiltro.TextChanged += (s, e) => AplicarFiltro();

            btnRefresh.Text = "Atualizar";
            btnRefresh.Size = new Size(90, 28);
            btnRefresh.Visible = false;
            btnRefresh.Click += (s, e) => AtualizarListaInterna();

            pnlMain.Controls.AddRange(new Control[] { lblFiltro, txtFiltro, btnRefresh, dgvClientes });

            // --- Dashboard cards (inicialização visual) ---
            InitializeDashboardCards();
            pnlMain.Controls.AddRange(new Control[] {
                cardCadastrados, cardEditados, cardDeletados
            });

            // Posiciona inicialmente
            PositionCadastroControls();
            PositionGridControls();
            PositionDashboardCards();
        }

        private void InitializeDashboardCards()
        {
            // Card base style
            Action<Panel> styleCard = (p) =>
            {
                p.Size = new Size(220, 120);
                p.BackColor = Color.White;
                p.BorderStyle = BorderStyle.FixedSingle;
                p.Padding = new Padding(12);
                p.Visible = false;
                // sombra simples (simulada com margin + BackColor do form)
            };

            // Cadastrados
            styleCard(cardCadastrados);
            cardCadastradosTitle.Text = "Clientes Cadastrados";
            cardCadastradosTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            cardCadastradosTitle.ForeColor = Color.Gray;
            cardCadastradosTitle.AutoSize = true;
            cardCadastradosTitle.Location = new Point(12, 8);

            cardCadastradosValue.Text = "0";
            cardCadastradosValue.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            cardCadastradosValue.ForeColor = Color.FromArgb(30, 144, 255);
            cardCadastradosValue.AutoSize = true;
            cardCadastradosValue.Location = new Point(12, 32);

            cardCadastradosSubtitle.Text = "Total de cadastros";
            cardCadastradosSubtitle.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            cardCadastradosSubtitle.ForeColor = Color.Gray;
            cardCadastradosSubtitle.AutoSize = true;
            cardCadastradosSubtitle.Location = new Point(12, 80);

            cardCadastrados.Controls.AddRange(new Control[] { cardCadastradosTitle, cardCadastradosValue, cardCadastradosSubtitle });

            // Editados
            styleCard(cardEditados);
            cardEditadosTitle.Text = "Clientes Editados";
            cardEditadosTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            cardEditadosTitle.ForeColor = Color.Gray;
            cardEditadosTitle.AutoSize = true;
            cardEditadosTitle.Location = new Point(12, 8);

            cardEditadosValue.Text = "0";
            cardEditadosValue.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            cardEditadosValue.ForeColor = Color.FromArgb(255, 165, 0); // laranja
            cardEditadosValue.AutoSize = true;
            cardEditadosValue.Location = new Point(12, 32);

            cardEditadosSubtitle.Text = "Total de edições";
            cardEditadosSubtitle.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            cardEditadosSubtitle.ForeColor = Color.Gray;
            cardEditadosSubtitle.AutoSize = true;
            cardEditadosSubtitle.Location = new Point(12, 80);

            cardEditados.Controls.AddRange(new Control[] { cardEditadosTitle, cardEditadosValue, cardEditadosSubtitle });

            // Deletados
            styleCard(cardDeletados);
            cardDeletadosTitle.Text = "Clientes Apagados";
            cardDeletadosTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            cardDeletadosTitle.ForeColor = Color.Gray;
            cardDeletadosTitle.AutoSize = true;
            cardDeletadosTitle.Location = new Point(12, 8);

            cardDeletadosValue.Text = "0";
            cardDeletadosValue.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            cardDeletadosValue.ForeColor = Color.FromArgb(220, 53, 69); // vermelho suave
            cardDeletadosValue.AutoSize = true;
            cardDeletadosValue.Location = new Point(12, 32);

            cardDeletadosSubtitle.Text = "Total de exclusões";
            cardDeletadosSubtitle.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            cardDeletadosSubtitle.ForeColor = Color.Gray;
            cardDeletadosSubtitle.AutoSize = true;
            cardDeletadosSubtitle.Location = new Point(12, 80);

            cardDeletados.Controls.AddRange(new Control[] { cardDeletadosTitle, cardDeletadosValue, cardDeletadosSubtitle });
        }

        private void ConfigureTextBox(TextBox tb)
        {
            tb.Enabled = true;
            tb.ReadOnly = false;
            tb.UseSystemPasswordChar = false;
            tb.BackColor = Color.White;
            tb.ForeColor = Color.Black;
            tb.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.BringToFront();
            tb.TabStop = true;
            tb.ShortcutsEnabled = true;
            tb.TextAlign = HorizontalAlignment.Left;

            tb.GotFocus += (s, e) =>
            {
                tb.SelectionStart = tb.Text?.Length ?? 0;
                tb.SelectionLength = 0;
                tb.ForeColor = Color.Black;
            };

            tb.TextChanged += (s, e) =>
            {
                if (!string.IsNullOrEmpty(tb.Text))
                {
                    tb.ForeColor = Color.Black;
                    tb.Invalidate();
                    tb.SelectionStart = tb.Text.Length;
                    tb.SelectionLength = 0;
                }
            };
        }

        // Centraliza o painel de cadastro dentro de pnlMain
        private void PositionCadastroControls()
        {
            if (pnlMain.ClientSize.Width <= 0 || pnlMain.ClientSize.Height <= 0) return;

            int panelWidth = Math.Min(pnlCadastro.Width, pnlMain.ClientSize.Width - 40);
            int panelHeight = pnlCadastro.Height;

            int padding = 20;
            int innerWidth = panelWidth - padding * 2;

            txtNome.Width = innerWidth;
            txtEmail.Width = Math.Min(360, innerWidth - 0);
            txtTelefone.Width = Math.Min(260, innerWidth - txtEmail.Width - 20);

            lblNome.Location = new Point(padding, 6);
            txtNome.Location = new Point(padding, 28);

            lblEmail.Location = new Point(padding, 68);
            txtEmail.Location = new Point(padding, 90);

            lblTelefone.Location = new Point(padding + txtEmail.Width + 20, 68);
            txtTelefone.Location = new Point(padding + txtEmail.Width + 20, 90);

            btnSalvar.Location = new Point(padding, 140);

            int requiredWidth = Math.Max(txtNome.Width, txtEmail.Width + txtTelefone.Width + 20) + padding * 2;
            pnlCadastro.Size = new Size(requiredWidth, panelHeight);

            int left = (pnlMain.ClientSize.Width - pnlCadastro.Width) / 2;
            int top = (pnlMain.ClientSize.Height - pnlCadastro.Height) / 2;
            if (left < 10) left = 10;
            if (top < 10) top = 10;
            pnlCadastro.Location = new Point(left, top);
            pnlCadastro.Visible = true;
        }

        // Posiciona e centraliza o DataGridView e o filtro
        private void PositionGridControls()
        {
            // largura máxima do grid
            int maxGridWidth = Math.Min(820, pnlMain.ClientSize.Width - 40);
            int gridWidth = Math.Max(600, maxGridWidth);
            int gridHeight = Math.Max(300, pnlMain.ClientSize.Height - 200);

            // posição central
            int left = (pnlMain.ClientSize.Width - gridWidth) / 2;
            int top = (pnlMain.ClientSize.Height - gridHeight) / 2;

            // coloca filtro acima do grid, alinhado ao centro do grid
            lblFiltro.Location = new Point(left, top - 36);
            txtFiltro.Location = new Point(left + lblFiltro.Width + 8, top - 40);
            btnRefresh.Location = new Point(left + lblFiltro.Width + txtFiltro.Width + 16, top - 40);

            dgvClientes.SetBounds(left, top, gridWidth, gridHeight);
        }

        // Posiciona os cards da dashboard centralizados e com espaçamento uniforme
        private void PositionDashboardCards()
        {
            int cardWidth = 240;
            int cardHeight = 120;
            int spacing = 24;

            cardCadastrados.Size = new Size(cardWidth, cardHeight);
            cardEditados.Size = new Size(cardWidth, cardHeight);
            cardDeletados.Size = new Size(cardWidth, cardHeight);

            int totalWidth = cardWidth * 3 + spacing * 2;
            int leftStart = (pnlMain.ClientSize.Width - totalWidth) / 2;
            if (leftStart < 10) leftStart = 10;
            int top = Math.Max(40, (pnlMain.ClientSize.Height - cardHeight) / 2);

            cardCadastrados.Location = new Point(leftStart, top);
            cardEditados.Location = new Point(leftStart + cardWidth + spacing, top);
            cardDeletados.Location = new Point(leftStart + (cardWidth + spacing) * 2, top);

            // garante visibilidade
            cardCadastrados.Visible = cardEditados.Visible = cardDeletados.Visible = true;
        }

        #region Cadastro / Salvar
        private void BtnSalvar_Click(object? sender, EventArgs e)
        {
            var nome = txtNome.Text?.Trim() ?? "";
            var email = txtEmail.Text?.Trim() ?? "";
            var telefone = txtTelefone.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nome))
            {
                MessageBox.Show("Nome é obrigatório.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNome.Focus();
                return;
            }

            if (emEdicao)
            {
                var cliente = new Cliente { Id = editarId, Nome = nome, Email = email, Telefone = telefone };
                repo.Atualizar(cliente);
                emEdicao = false;
                editarId = 0;
                MessageBox.Show("Cliente atualizado com sucesso.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var cliente = new Cliente { Nome = nome, Email = email, Telefone = telefone };
                repo.Adicionar(cliente);
                MessageBox.Show("Cliente cadastrado com sucesso.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            LimparCamposCadastro();
            AtualizarListaInterna();
            MostrarDashboard();
        }

        private void LimparCamposCadastro()
        {
            txtNome.Text = "";
            txtEmail.Text = "";
            txtTelefone.Text = "";
        }
        #endregion

        #region Listagem / Edit / Delete
        private void MostrarLista()
        {
            OcultarTodasAreas();
            lblFiltro.Visible = txtFiltro.Visible = btnRefresh.Visible = true;
            dgvClientes.Visible = true;
            PositionGridControls();
            AtualizarListaInterna();
        }

        private void AtualizarListaInterna()
        {
            var lista = repo.Listar();
            dgvClientes.DataSource = null;
            dgvClientes.DataSource = lista.Select(c => new {
                c.Id,
                c.Nome,
                c.Email,
                c.Telefone
            }).ToList();

            // ajusta colunas
            if (dgvClientes.Columns["Id"] != null)
            {
                dgvClientes.Columns["Id"].FillWeight = 10;
                dgvClientes.Columns["Id"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (dgvClientes.Columns["Nome"] != null)
            {
                dgvClientes.Columns["Nome"].FillWeight = 40;
                dgvClientes.Columns["Nome"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
            if (dgvClientes.Columns["Email"] != null)
            {
                dgvClientes.Columns["Email"].FillWeight = 30;
                dgvClientes.Columns["Email"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
            if (dgvClientes.Columns["Telefone"] != null)
            {
                dgvClientes.Columns["Telefone"].FillWeight = 20;
                dgvClientes.Columns["Telefone"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void AplicarFiltro()
        {
            var filtro = txtFiltro.Text?.Trim().ToLower() ?? "";
            if (string.IsNullOrEmpty(filtro))
            {
                AtualizarListaInterna();
                return;
            }

            var lista = repo.Listar()
                .Where(c => (c.Nome ?? "").ToLower().Contains(filtro)
                         || (c.Email ?? "").ToLower().Contains(filtro)
                         || (c.Telefone ?? "").ToLower().Contains(filtro))
                .Select(c => new { c.Id, c.Nome, c.Email, c.Telefone })
                .ToList();

            dgvClientes.DataSource = lista;
        }

        // Trata cliques nas colunas de botão (Editar / Apagar)
        private void DgvClientes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var colName = dgvClientes.Columns[e.ColumnIndex].Name;
            if (colName == "EditarBtn")
            {
                dgvClientes.ClearSelection();
                dgvClientes.Rows[e.RowIndex].Selected = true;
                IniciarEdicaoSelecionada();
            }
            else if (colName == "ApagarBtn")
            {
                dgvClientes.ClearSelection();
                dgvClientes.Rows[e.RowIndex].Selected = true;
                ApagarSelecionado();
            }
        }

        private void IniciarEdicaoSelecionada()
        {
            if (!dgvClientes.Visible || dgvClientes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecione um cliente na lista para editar.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvClientes.SelectedRows[0];
            var id = Convert.ToInt32(row.Cells["Id"].Value);
            var cliente = repo.Listar().FirstOrDefault(c => c.Id == id);
            if (cliente == null)
            {
                MessageBox.Show("Cliente não encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            emEdicao = true;
            editarId = cliente.Id;
            txtNome.Text = cliente.Nome;
            txtEmail.Text = cliente.Email;
            txtTelefone.Text = cliente.Telefone;
            MostrarCadastro();
            txtNome.Focus();
            txtNome.SelectionStart = txtNome.Text.Length;
        }

        private void ApagarSelecionado()
        {
            if (!dgvClientes.Visible || dgvClientes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecione um cliente na lista para apagar.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvClientes.SelectedRows[0];
            var id = Convert.ToInt32(row.Cells["Id"].Value);
            var cliente = repo.Listar().FirstOrDefault(c => c.Id == id);
            if (cliente == null)
            {
                MessageBox.Show("Cliente não encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var resp = MessageBox.Show($"Deseja realmente apagar o cliente '{cliente.Nome}' (Id {cliente.Id})?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (resp == DialogResult.Yes)
            {
                repo.Remover(cliente.Id);
                AtualizarListaInterna();
                MostrarDashboard();
            }
        }
        #endregion

        #region Dashboard
        private void MostrarDashboard()
        {
            OcultarTodasAreas();

            // obtém stats
            var stats = repo.ObterStats();

            // atualiza valores nos cards
            cardCadastradosValue.Text = stats.Cadastrados.ToString();
            cardEditadosValue.Text = stats.Editados.ToString();
            cardDeletadosValue.Text = stats.Deletados.ToString();

            // opcional: destaque visual se algum contador > 0
            cardCadastrados.BackColor = stats.Cadastrados > 0 ? Color.White : Color.FromArgb(250, 250, 250);
            cardEditados.BackColor = stats.Editados > 0 ? Color.White : Color.FromArgb(250, 250, 250);
            cardDeletados.BackColor = stats.Deletados > 0 ? Color.White : Color.FromArgb(250, 250, 250);

            // posiciona e mostra os cards
            PositionDashboardCards();
        }
        #endregion

        #region Navegação / Helpers
        private void MostrarCadastro()
        {
            OcultarTodasAreas();
            pnlCadastro.Visible = true;
            PositionCadastroControls();
        }

        private void OcultarTodasAreas()
        {
            foreach (Control c in pnlMain.Controls)
            {
                c.Visible = false;
            }
            pnlCadastro.Visible = false;
            dgvClientes.Visible = false;
            lblFiltro.Visible = txtFiltro.Visible = btnRefresh.Visible = false;
            cardCadastrados.Visible = cardEditados.Visible = cardDeletados.Visible = false;
        }
        #endregion
    }
}
