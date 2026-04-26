# BeaverTech Tracker - Java (publicacao)

Publicacao do pacote `com.beavertech:tracker-java`.

## Preparar release

1. Atualize a versao em `sdk/java/pom.xml`.
2. Revise `sdk/java/README.md`.
3. Faça tag git (ex: `v0.1.0`).

## Publicar em repositorio privado (Nexus/Artifactory)

Configure o `distributionManagement` no `pom.xml` e execute:

```bash
mvn -f sdk/java/pom.xml deploy
```

## Publicar no Maven Central (resumo)

1. Configure `groupId` no Sonatype.
2. Adicione assinatura GPG e `distributionManagement`.
3. Execute o deploy:

```bash
mvn -f sdk/java/pom.xml deploy -P release
```

Obs: Maven Central exige GPG + verificacoes de metadata.
