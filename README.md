# Proyecto inmobiliaria: reservas temporales

## Integrantes

Erica Castro
-correo: erica.castro.0615@gmail.com
-usuario github: [eriadr0615](https://github.com/eriadr0615)
-usuario discord: erica_19340

Sebastian Castro
-correo: castrosebastian87@gmail.com
-usuario github: [ssebasss](https://github.com/ssebasss)

## Descripcion

sistema web gestado para la administración de alquileres temporarios regentiado por una inmobiliaria.

### Modelado de datos

Esquema de modelo de datos perteneciente a la app:

#### Primera entrega :

- alta, baja, modificación de entidad Propietario
- alta, baja, modificacion de entidad Inquilinos

#### Diagrama

Diagrama de entidad relacion

![Diagrama de Entidad Relación](./DER.png)

## Base de datos
### Configuración de la base de datos

El proyecto utiliza **MySQL** como motor de base de datos.

El archivo `reservas_temporales.sql`, ubicado en la raíz del proyecto, contiene las sentencias necesarias para crear e inicializar la base de datos.

#### 1. Crear la base de datos

Abrir MySQL Workbench o un cliente compatible con MySQL y ejecutar el archivo:

```text
reservas_temporales.sql
```

El script crea automáticamente la base de datos:

```text
reservas_temporales
```

y las tablas necesarias para el sistema.

> Importante: el script contiene `DROP DATABASE IF EXISTS reservas_temporales`, por lo que volver a ejecutarlo elimina la base existente y la crea nuevamente.

#### 2. Configurar la conexión

Por seguridad, la contraseña de MySQL no se almacena en el repositorio.

Desde una terminal ubicada en la carpeta del proyecto ejecutar:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=reservas_temporales;User=root;Password=TU_CONTRASEÑA;"
```

Reemplazar `TU_CONTRASEÑA` por la contraseña correspondiente al usuario de MySQL.

Si el usuario `root` no posee contraseña:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=reservas_temporales;User=root;Password=;"
```

#### 3. Restaurar dependencias

```bash
dotnet restore
```

#### 4. Ejecutar el proyecto

```bash
dotnet run
```

Con MySQL en ejecución y la cadena de conexión configurada, la aplicación podrá acceder a la base de datos mediante ADO.NET y `MySqlConnector`.

