# TPPreenchedor

Aplicativo WinForms (.NET Framework 4.7.2) para preenchimento rapido de credenciais e textos, com autenticacao de usuarios e persistencia em SQLite.

## Funcionalidades

- Login de usuarios com senha criptografada (PBKDF2).
- Restauracao de acesso do admin inicial.
- Gestao de usuarios (criar, editar, ativar/desativar, redefinir senha, excluir).
- Cadastro de itens por usuario:
- Credenciais (texto curto).
- Textos longos (blocos para preenchimento).
- Preenchimento automatico em qualquer janela apos tempo configuravel.

## Requisitos

- Windows.
- .NET SDK instalado (ou Visual Studio com workload .NET desktop).

## Como executar

1. Restaurar pacotes (se necessario):

```powershell
dotnet restore TPPreenchedor.sln
```

2. Compilar:

```powershell
dotnet build TPPreenchedor.sln -c Debug
```

3. Executar o binario gerado:

```powershell
.\bin\Debug\TPPreenchedor.exe
```

## Banco de dados

- Provider atual: `sqlite`.
- Caminho padrao: `Database\TpPreenchedor.db`.
- Configuracao em [App.config](C:\Projetos\Preenchedor_Password\App.config).
- O bootstrap cria schema e seed inicial automaticamente ao iniciar.

## Acesso inicial

- Login: `admin`
- Senha: `admin123`

Caso necessario, use o botao `Restaurar admin` na tela de login.

## Estrutura principal

- [Program.cs](C:\Projetos\Preenchedor_Password\Program.cs): inicializacao da aplicacao e fluxo de login.
- [Data/DatabaseBootstrapper.cs](C:\Projetos\Preenchedor_Password\Data\DatabaseBootstrapper.cs): criacao de schema e seed inicial.
- [Forms/LoginForm.cs](C:\Projetos\Preenchedor_Password\Forms\LoginForm.cs): autenticacao.
- [Forms/Preenchedor.cs](C:\Projetos\Preenchedor_Password\Forms\Preenchedor.cs): tela principal.
- [Forms/UserManagementForm.cs](C:\Projetos\Preenchedor_Password\Forms\UserManagementForm.cs): gestao de usuarios.
- [Forms/ChangePasswordForm.cs](C:\Projetos\Preenchedor_Password\Forms\ChangePasswordForm.cs): troca de senha.
