using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class AllAsset
    {
        public long Id { get; set; }
        /// <summary>
        /// Tipo de activo: transformador, interruptor, reconectador o seccionador
        /// </summary>
        public string? TypeAsset { get; set; }
        /// <summary>
        /// Código de identificación del activo dentro del circuito.
        /// Depende de la posición geográfica y no varía si se reemplaza el activo
        /// </summary>
        public string? CodeSig { get; set; }
        /// <summary>
        /// Código de identificación del activo; está asociado al code_sig
        /// Varía si se reemplaza el activo
        /// </summary>
        public string? Uia { get; set; }
        /// <summary>
        /// Código taxonómico; utilizado para identificar el activo en el software Máximo
        /// </summary>
        public string? Codetaxo { get; set; }
        /// <summary>
        /// Código del circuito al cual pertenece el activo
        /// </summary>
        public string? Fparent { get; set; }
        /// <summary>
        /// Latitud; ubicación geográfica del activo
        /// </summary>
        public float? Latitude { get; set; }
        /// <summary>
        /// Longitud; ubicación geográfica del activo
        /// </summary>
        public float? Longitude { get; set; }
        public string? Poblation { get; set; }
        /// <summary>
        /// Grupo de calidad del activo; de acuerdo a la CREG 015
        /// 
        /// Primer Dígito: Criticidad del Activo
        /// 1: Alta Criticidad
        /// 2: Media Criticidad
        /// 3: Baja Criticidad
        /// 
        /// Segundo Dígito: Nivel de Tensión
        /// 1: Alta Tensión (AT)
        /// 2: Media Tensión (MT)
        /// 3: Baja Tensión (BT)
        /// </summary>
        public string? Group015 { get; set; }
        /// <summary>
        /// Unidad constructiva del activo;  de acuerdo al capítulo 14 de la CREG 015.
        /// 
        /// Es una agrupación lógica de componentes que funcionan juntos como una unidad única y que tienen un propósito específico dentro del sistema eléctrico.
        /// 
        /// En DANNTE lo utilizamos para identificar el tiempo de vida util de un activo de acuerdo a la CREG
        /// </summary>
        public string? Uccap14 { get; set; }
        /// <summary>
        /// Fecha de instalación del activo
        /// </summary>
        public DateOnly? DateInst { get; set; }
        /// <summary>
        /// Fecha de desinstalación de activo
        /// </summary>
        public DateOnly? DateUnin { get; set; }
        public int? State { get; set; }
        public long? IdZone { get; set; }
        public string? NameZone { get; set; }
        public long? IdRegion { get; set; }
        public string? NameRegion { get; set; }
        public long? IdLocality { get; set; }
        public string? NameLocality { get; set; }
        public long? IdSector { get; set; }
        public string? NameSector { get; set; }
        /// <summary>
        /// Dirección de la ubicación del activo
        /// </summary>
        public string? Address { get; set; }
        public long? GeographicalCode { get; set; }
    }
}
