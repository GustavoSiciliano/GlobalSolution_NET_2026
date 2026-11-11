
using System;
using Binova.Api.Dados;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Binova.Api.Migrations;

[DbContext(typeof(BinovaDbContext))]
[Migration("20261109000100_CriacaoInicial")]
public partial class CriacaoInicial : Migration
{
    // Cria somente as estruturas que ainda não existem no schema Oracle
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Tabela de usuários
        migrationBuilder.Sql("""
            DECLARE
                tabelaExiste NUMBER;
            BEGIN
                SELECT COUNT(*)
                  INTO tabelaExiste
                  FROM USER_TABLES
                 WHERE TABLE_NAME = 'USUARIO';

                IF tabelaExiste = 0 THEN
                    EXECUTE IMMEDIATE q'[
                        CREATE TABLE USUARIO (
                            ID NUMBER PRIMARY KEY,
                            NOME VARCHAR2(120) NOT NULL,
                            EMAIL VARCHAR2(180) UNIQUE NOT NULL,
                            SENHA_HASH VARCHAR2(255) NOT NULL,
                            PAPEL VARCHAR2(20) DEFAULT 'CIDADAO' NOT NULL,
                            CRIADO_EM TIMESTAMP WITH TIME ZONE
                                DEFAULT SYSTIMESTAMP NOT NULL,

                            CHECK (PAPEL IN ('CIDADAO', 'ADMIN')),
                            CHECK (TRIM(NOME) IS NOT NULL)
                        )
                    ]';
                END IF;
            END;
            """);

        // Tabela de pontos de coleta
        migrationBuilder.Sql("""
            DECLARE
                tabelaExiste NUMBER;
            BEGIN
                SELECT COUNT(*)
                  INTO tabelaExiste
                  FROM USER_TABLES
                 WHERE TABLE_NAME = 'PONTO_COLETA';

                IF tabelaExiste = 0 THEN
                    EXECUTE IMMEDIATE q'[
                        CREATE TABLE PONTO_COLETA (
                            ID NUMBER PRIMARY KEY,
                            NOME VARCHAR2(120) NOT NULL,
                            DESCRICAO_LOCALIZACAO VARCHAR2(255) NOT NULL,
                            CAPACIDADE_LITROS NUMBER(8,2) NOT NULL,
                            ATIVO NUMBER(1) DEFAULT 1 NOT NULL,
                            CRIADO_EM TIMESTAMP WITH TIME ZONE
                                DEFAULT SYSTIMESTAMP NOT NULL,

                            CHECK (CAPACIDADE_LITROS > 0),
                            CHECK (ATIVO IN (0, 1)),
                            CHECK (TRIM(NOME) IS NOT NULL),
                            CHECK (TRIM(DESCRICAO_LOCALIZACAO) IS NOT NULL)
                        )
                    ]';
                END IF;
            END;
            """);

        // Tabela de leituras dos sensores
        migrationBuilder.Sql("""
            DECLARE
                tabelaExiste NUMBER;
            BEGIN
                SELECT COUNT(*)
                  INTO tabelaExiste
                  FROM USER_TABLES
                 WHERE TABLE_NAME = 'LEITURA_SENSOR';

                IF tabelaExiste = 0 THEN
                    EXECUTE IMMEDIATE q'[
                        CREATE TABLE LEITURA_SENSOR (
                            ID NUMBER PRIMARY KEY,
                            PONTO_COLETA_ID NUMBER NOT NULL
                                REFERENCES PONTO_COLETA(ID),
                            DISTANCIA_CM NUMBER(8,2) NOT NULL,
                            NIVEL_PERCENTUAL NUMBER(5,2) NOT NULL,
                            REGISTRADO_EM TIMESTAMP WITH TIME ZONE
                                DEFAULT SYSTIMESTAMP NOT NULL,

                            CHECK (DISTANCIA_CM > 0),
                            CHECK (NIVEL_PERCENTUAL BETWEEN 0 AND 100)
                        )
                    ]';
                END IF;
            END;
            """);

        // Tabela de ocorrências registradas pelos cidadãos
        migrationBuilder.Sql("""
            DECLARE
                tabelaExiste NUMBER;
            BEGIN
                SELECT COUNT(*)
                  INTO tabelaExiste
                  FROM USER_TABLES
                 WHERE TABLE_NAME = 'OCORRENCIA';

                IF tabelaExiste = 0 THEN
                    EXECUTE IMMEDIATE q'[
                        CREATE TABLE OCORRENCIA (
                            ID NUMBER PRIMARY KEY,
                            PONTO_COLETA_ID NUMBER NOT NULL
                                REFERENCES PONTO_COLETA(ID),
                            USUARIO_ID NUMBER NOT NULL
                                REFERENCES USUARIO(ID),
                            TIPO VARCHAR2(32) NOT NULL,
                            DESCRICAO VARCHAR2(1000) NOT NULL,
                            STATUS VARCHAR2(20)
                                DEFAULT 'ABERTA' NOT NULL,
                            CRIADO_EM TIMESTAMP WITH TIME ZONE
                                DEFAULT SYSTIMESTAMP NOT NULL,
                            ATUALIZADO_EM TIMESTAMP WITH TIME ZONE
                                DEFAULT SYSTIMESTAMP NOT NULL,

                            CHECK (TIPO IN (
                                'LIXEIRA_CHEIA',
                                'DESCARTE_IRREGULAR',
                                'ESTRUTURA_DANIFICADA',
                                'OUTRO'
                            )),
                            CHECK (STATUS IN (
                                'ABERTA',
                                'EM_ATENDIMENTO',
                                'RESOLVIDA'
                            )),
                            CHECK (TRIM(DESCRICAO) IS NOT NULL)
                        )
                    ]';
                END IF;
            END;
            """);

        // Sequência para novos usuários
        migrationBuilder.Sql("""
            DECLARE
                sequenciaExiste NUMBER;
                proximoId NUMBER;
            BEGIN
                SELECT COUNT(*)
                  INTO sequenciaExiste
                  FROM USER_SEQUENCES
                 WHERE SEQUENCE_NAME = 'BINOVA_USUARIO_SEQ';

                IF sequenciaExiste = 0 THEN
                    SELECT NVL(MAX(ID), 0) + 1
                      INTO proximoId
                      FROM USUARIO;

                    EXECUTE IMMEDIATE
                        'CREATE SEQUENCE BINOVA_USUARIO_SEQ ' ||
                        'START WITH ' || proximoId ||
                        ' INCREMENT BY 1 NOCACHE';
                END IF;
            END;
            """);

        // Sequência para novos pontos de coleta
        migrationBuilder.Sql("""
            DECLARE
                sequenciaExiste NUMBER;
                proximoId NUMBER;
            BEGIN
                SELECT COUNT(*)
                  INTO sequenciaExiste
                  FROM USER_SEQUENCES
                 WHERE SEQUENCE_NAME = 'BINOVA_PONTO_SEQ';

                IF sequenciaExiste = 0 THEN
                    SELECT NVL(MAX(ID), 0) + 1
                      INTO proximoId
                      FROM PONTO_COLETA;

                    EXECUTE IMMEDIATE
                        'CREATE SEQUENCE BINOVA_PONTO_SEQ ' ||
                        'START WITH ' || proximoId ||
                        ' INCREMENT BY 1 NOCACHE';
                END IF;
            END;
            """);

        // Sequência para novas leituras
        migrationBuilder.Sql("""
            DECLARE
                sequenciaExiste NUMBER;
                proximoId NUMBER;
            BEGIN
                SELECT COUNT(*)
                  INTO sequenciaExiste
                  FROM USER_SEQUENCES
                 WHERE SEQUENCE_NAME = 'BINOVA_LEITURA_SEQ';

                IF sequenciaExiste = 0 THEN
                    SELECT NVL(MAX(ID), 0) + 1
                      INTO proximoId
                      FROM LEITURA_SENSOR;

                    EXECUTE IMMEDIATE
                        'CREATE SEQUENCE BINOVA_LEITURA_SEQ ' ||
                        'START WITH ' || proximoId ||
                        ' INCREMENT BY 1 NOCACHE';
                END IF;
            END;
            """);

        // Sequência para novas ocorrências
        migrationBuilder.Sql("""
            DECLARE
                sequenciaExiste NUMBER;
                proximoId NUMBER;
            BEGIN
                SELECT COUNT(*)
                  INTO sequenciaExiste
                  FROM USER_SEQUENCES
                 WHERE SEQUENCE_NAME = 'BINOVA_OCORRENCIA_SEQ';

                IF sequenciaExiste = 0 THEN
                    SELECT NVL(MAX(ID), 0) + 1
                      INTO proximoId
                      FROM OCORRENCIA;

                    EXECUTE IMMEDIATE
                        'CREATE SEQUENCE BINOVA_OCORRENCIA_SEQ ' ||
                        'START WITH ' || proximoId ||
                        ' INCREMENT BY 1 NOCACHE';
                END IF;
            END;
            """);
    }

    // Impede a reversão automática das estruturas compartilhadas
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "Esta Migration não pode ser revertida automaticamente, " +
            "pois utiliza tabelas compartilhadas do projeto Binova."
        );
    }
}
