using System;
using System.Windows.Forms;

public class FormCadastro : Form
{
    private TextBox txtNome = new TextBox();
    private TextBox txtEmail = new TextBox();
    private TextBox txtTelefone = new TextBox();
    private Button btnSalvar = new Button();
    private ListBox lstClientes = new ListBox();
    private ClienteRepository repo = new ClienteRepository();

    public FormCadastro()
    {
        this.Text = "Cadastro de Clientes";
        this.Size = new System.Drawing.Size(400, 400);

        Label lblNome = new Label { Text = "Nome:", Top = 20, Left = 20 };
        txtNome.SetBounds(80, 20, 200, 20);

        Label lblEmail = new Label { Text = "Email:", Top = 60, Left = 20 };
        txtEmail.SetBounds(80, 60, 200, 20);

        Label lblTelefone = new Label { Text = "Telefone:", Top = 100, Left = 20 };
        txtTelefone.SetBounds(80, 100, 200, 20);

        btnSalvar.Text = "Salvar";
        btnSalvar.SetBounds(80, 140, 100, 30);
        btnSalvar.Click += BtnSalvar_Click;

        lstClientes.SetBounds(20, 200, 340, 120);

        this.Controls.AddRange(new Control[] { lblNome, txtNome, lblEmail, txtEmail, lblTelefone, txtTelefone, btnSalvar, lstClientes });

        AtualizarLista();
    }

    private void BtnSalvar_Click(object sender, EventArgs e)
    {
        var cliente = new Cliente
        {
            Nome = txtNome.Text,
            Email = txtEmail.Text,
            Telefone = txtTelefone.Text
        };
        repo.Adicionar(cliente);
        AtualizarLista();
        txtNome.Clear();
        txtEmail.Clear();
        txtTelefone.Clear();
    }

    private void AtualizarLista()
    {
        lstClientes.Items.Clear();
        foreach (var cliente in repo.Listar())
        {
            lstClientes.Items.Add($"{cliente.Id} - {cliente.Nome} - {cliente.Email} - {cliente.Telefone}");
        }
    }
}
