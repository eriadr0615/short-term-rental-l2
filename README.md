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

Abrir MySQL Workbench o un app compatible con MySQL y ejecuta el archivo:

```text
reservas_temporales.sql
```

El script crea automáticamente la base de datos:

```text
reservas_temporales
```

y las tablas necesarias para el sistema.

> Importante: el script contiene `DROP DATABASE IF EXISTS reservas_temporales`, por lo que volver a ejecutarlo elimina la base existente y la crea nuevamente!

#### 2. Configurar la conexión

Por seguridad, la contraseña de MySQL no se almacena en el repositorio.

Abrir el archivo:

```text
appsettings.json
```

y modificar la cadena de conexión según el usuario y contraseña de MySQL de cada computadora.

Por ejemplo:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=reservas_temporales;User=root;Password=CAMBIAR_CLAVE;SslMode=None;"
}
```

> **OJO:** reemplazar `CAMBIAR_CLAVE` por la contraseña configurada en MySQL.

Por ejemplo, si la contraseña es `123`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=reservas_temporales;User=root;Password=123;SslMode=None;"
}
```

Si el usuario `root` no posee contraseña:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=reservas_temporales;User=root;Password=;SslMode=None;"
}
```

#### 3. Restaurar dependencias

Desde una terminal ubicada en la carpeta del proyecto ejecutar:

```bash
dotnet restore
```

Este comando descarga y restaura los paquetes necesarios para ejecutar el proyecto.

#### 4. Ejecutar el proyecto

En la misma terminal ejecutar:

```bash
dotnet run
```

La consola de la terminal, mostrará la dirección donde se está ejecutando la aplicación. Por ejemplo:

```text
http://localhost:5277
```

Listo! Con esa url ya podes ingresar a la aplicación desde el navegador.
