# Cadastro de Clientes em C#

Este projeto é um sistema simples de **cadastro de clientes** desenvolvido em **C#**, utilizando **Windows Forms** para a interface gráfica e **SQLite** como banco de dados local.

---

## 📋 Funcionalidades

- Cadastrar clientes com **Nome**, **Email** e **Telefone**.
- Listar todos os clientes cadastrados.
- Banco de dados **SQLite** criado automaticamente (`clientes.db`).
- Interface gráfica amigável em **Windows Forms**.
- Organização em módulos:
  - `Cliente.cs` → classe de dados.
  - `ClienteRepository.cs` → acesso ao banco de dados.
  - `FormCadastro.cs` → interface gráfica.
  - `Program.cs` → inicialização da aplicação.

---

## ⚙️ Requisitos

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Visual Studio Code](https://code.visualstudio.com/) ou outro editor
- Pacote NuGet **System.Data.SQLite**

---

## 🚀 Como rodar o projeto

### 1. Clonar o repositório
```bash
git clone https://github.com/matrix0377/cad_clientes_csharp.git
cd cad_clientes_csharp

