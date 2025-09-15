using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_server.Migrations
{
    /// <inheritdoc />
    public partial class AddTelefoneColumnFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Verificar se a coluna já existe antes de adicionar
            migrationBuilder.Sql(@"
                SET @col_exists = 0;
                SELECT COUNT(*) INTO @col_exists 
                FROM information_schema.columns 
                WHERE table_schema = DATABASE() 
                AND table_name = 'Usuarios' 
                AND column_name = 'Telefone';
                
                SET @sql = IF(@col_exists = 0, 
                    'ALTER TABLE Usuarios ADD COLUMN Telefone LONGTEXT NOT NULL DEFAULT ''''', 
                    'SELECT ''Column already exists'' as Info');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
