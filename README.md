<h2>API de Autenticação com ASP.NET Identity</h2>

O objetivo dessa API é realizar o gerenciamento de usuários como; Criar conta, Fazer login, Confirmação de email, Reset de senha e ETC.
Pode ser facilmente implementada em um projeto que necessita de uma API de autenticação. Abaixo tem dicas de como utiliza-lá.

## 🚀 Tecnologias Utilizadas: 
- DotNet 8: para construção do projeto.
- PostgreSQL: para gerencimento e armazenamento dos dados.
- ASP.NET Identity: melhor gerencimento de usuários e informações.
- Docker: gerenciar aplicação em contêiner.

## 💻 Primeiros passos
*Clonar o projeto e rodar localmente*

1. Necessário que o `dotnet` e o `PostgreSQL` esteja devidamente instalado em sua máquina.
2. Execute o comando `git clone https://github.com/leeo-sf/identity.git` no terminal.
3. No arquivo `src/PlanWise.Identity/appsettings.json` insira a URL de conexão com o PostgreSQL em `ConnectionStrings__DefaultConnectionDb`.
4. Execute o comando `Update-Database` para replicar o banco de dados em sua máquina.
5. Execute o comando `dotnet run` para executar o projeto.

## Executar projeto com o docker
*Em breve*
