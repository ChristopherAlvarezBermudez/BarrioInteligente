using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BarrioInteligenteWeb.Data;
using BarrioInteligenteWeb.Models;

namespace BarrioInteligenteWeb.Services
{
    public class ProfanityService : IProfanityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProfanityService> _logger;

        // ══════════════════════════════════════════════════════════════
        // HASHSET MASIVO — Jerga dominicana + variantes leetspeak
        // Instanciado con OrdinalIgnoreCase para matching sin importar casing
        // ══════════════════════════════════════════════════════════════
        private static readonly HashSet<string> _diccionarioLocal = new(StringComparer.OrdinalIgnoreCase)
        {
            // ── MMG / Mamaguevo y variantes ──
            "mmg","mmgvo","mmgv","mmgb","mmgwebo","mamaguevo","mamaguebo","mamagwebo",
            "mamahuevo","mamahuebo","mamagüevo","m4m4gu3v0","m4m4gu3b0","m4m4hu3v0",
            "m4m4hu3b0","m4m4gu3w0","mamag","mmgvazo","mamaguevaso","mamahuevazo",
            "mamaguebazo","mmgbo","m-m-g","m.m.g","m_m_g","m.m.g.v.o","m4m4gb","m4m4gv0",
            "mamagu3vo","mamagv","m4m4g","m4m4gv","m4m4","mvmvgv3v0","mvmv",

            // ── Coño y variantes ──
            "coño","cono","c0n0","c0ñ0","coñazo","conazo","coñito","conito",
            "c.o.n.o","c_o_n_o","c.o.ñ.o","cooono","cooño","c00ñ0","coñaso","conaso",
            "c0ñas0","c_o_ñ_o","c.0.ñ.0",

            // ── Diablo y variantes ──
            "diablo","diablazo","d14bl0","d1ablo","d1abl0","diabl0","d14blo","diantre",
            "diache","diablaso","d14blas0","d.i.a.b.l.o","d_i_a_b_l_o","diablito","diablita",

            // ── Maldito y variantes ──
            "maldito","mardito","mrdito","m4ld1t0","m4rd1t0","maldita","mardita","mrdita",
            "m4ld1t4","m4rd1t4","mldito","mldita","malditazo","marditaso",
            "m.a.l.d.i.t.o","m.a.r.d.i.t.o","m_a_l_d_i_t_o","malditasea","m4ld1t4s34",

            // ── Azaroso y variantes ──
            "azaroso","asaroso","azarosa","asarosa","4z4r0s0","4s4r0s0","azarozo",
            "asarozo","4z4r0z0","azarosazo","asarosaso","a.z.a.r.o.s.o","a_z_a_r_o_s_o","4z4r0z4",

            // ── Cuero y variantes ──
            "cuero","cuera","cuerazo","kuer0","cu3r0","cuerita","cver0","cueraso",
            "c.u.e.r.o","k.u.e.r.o","qwero","cwero","cwer0","kuero","kuera","kwer0","kwero",

            // ── Palomo y variantes ──
            "palomo","paloma","p4l0m0","palomazo","palomaso","p.a.l.o.m.o","p4lom0",
            "palomito","palomita","p_a_l_o_m_o","p4l0m4",

            // ── Pariguayo y variantes ──
            "pariguayo","pariguaya","p4r1gu4y0","parigüayo","pariguayaso","pariguayazo",
            "p4r1gu4y4","p.a.r.i.g.u.a.y.o","p_a_r_i_g_u_a_y_o","pariguayito","pariguayita",

            // ── Singar y variantes ──
            "singar","singa","cinga","cingando","singando","cingao","singao","singon",
            "cingon","singona","s1ng4","s1ng4r","c1ng4","s.i.n.g.a","c.i.n.g.a",
            "s_i_n_g_a","singadita","singadera","cingadera","s1ng4d3r4",

            // ── Rapa y variantes ──
            "rapa","rapar","rapando","r4p4","r4p4r","r4p4nd0","rapao","rapada","rapon",
            "rapona","r.a.p.a","r_a_p_a","rapita","rapadera","r4p4d3r4","rapasumadre","r4p4svm4dr3",

            // ── Chopo y variantes ──
            "chopo","chopa","ch0p0","chopazo","choperia","ch0p4","chopita","chopito",
            "c.h.o.p.o","c_h_o_p_o",

            // ── Lambón y variantes ──
            "lambon","l4mb0n","lambona","lambonismo","lambe","l4mb3","lambiendo",
            "l4mb13nd0","lambonaso","lambonazo","l.a.m.b.o.n","l_a_m_b_o_n",

            // ── Chapiadora y variantes ──
            "chapiadora","chapeadora","chapiador","chapeador","chapi","ch4p1",
            "ch4p14d0r4","ch4p34d0r4","chapiao","chapiada","ch.a.p.i","c_h_a_p_i",

            // ── Rastrero y variantes ──
            "rastrero","rastrera","r4str3r0","rastrerismo","r4str3r4","rastrerito",
            "r.a.s.t.r.e.r.o",

            // ── HDP y variantes ──
            "hdp","hijueputa","h1j0d3pvt4","h.d.p","h_d_p","h1jvpvt4","hijoeputa",
            "hijo_de_puta","hija_de_puta","jueputa","ju3pvt4",

            // ── Tuta y variantes ──
            "tuta","hijo_de_tuta","hijos_de_tuta","h1j0_d3_tvt4","t.u.t.a",

            // ── Toto y variantes ──
            "toto","totazo","t0t0","t0t4z0","totaso","t.o.t.o","t_o_t_o","totito",
            "totico","totote",

            // ── Guevo/Huevo y variantes ──
            "guevo","huevo","webo","wev0","w3b0","gu3v0","hu3v0","guebazo","huevazo",
            "huebaso","guebaso","g.u.e.v.o","g_u_e_v_o","guevito",

            // ── Pinga y variantes ──
            "pinga","p1ng4","pingazo","pingaso","p1ng4z0","p.i.n.g.a","p_i_n_g_a","pingota",

            // ── Mierda y variantes ──
            "mierda","m13rd4","mielda","m13ld4","mierdita","mierdaza","m13rd4z4",
            "m.i.e.r.d.a","m_i_e_r_d_a",

            // ── Jablador/Hablador ──
            "jablador","hablador","jabladorazo","j4bl4d0r","h4bl4d0r","jabladora",
            "habladora","jabladoras","habladoras","jabladoraso",

            // ── Barriga verde ──
            "barriga_verde","barrigaverde","b4rr1g4_v3rd3","b4rr1g4v3rd3","barriga-verde",

            // ── Insultos dominicanos únicos ──
            "cutafara","cutáfara","cvt4f4r4","c.u.t.a.f.a.r.a","c_u_t_a_f_a_r_a",
            "arracavaca","arracavacas","4rr4c4v4c4","a.r.r.a.c.a.v.a.c.a","a_r_r_a_c_a_v_a_c_a",
            "caremime","c4r3m1m3","c.a.r.e.m.i.m.e","c_a_r_e_m_i_m_e",
            "cacuemaco","c4cv3m4c0","c.a.c.u.e.m.a.c.o","c_a_c_u_e_m_a_c_o",
            "cacata","c4c4t4","c.a.c.a.t.a","hija_de_la_cacata","hijo_de_la_cacata","h1j0_d3_l4_c4c4t4",
            "piratanque","p1r4t4nqv3","p.i.r.a.t.a.n.q.u.e",
            "cojefiado","c0j3f14d0",

            // ── Estados emocionales ofensivos ──
            "emperrao","3mp3rr40","emperrada","encaquetarse","3nc4qv3t4rs3",
            "cocotazo","c0c0t4z0","cocotaso","c0c0t4s0",
            "tollo","t0ll0",
            "quillado","qvill4d0","quillao","qvill40","quillada",
            "encojonado","3nc0j0n4d0","encojonao","3nc0j0n40","encojonada",
            "jumo","jvm0","jumera","jvm3r4",

            // ── Insultos de carácter ──
            "baboso","b4b0s0","babosa","b4b0s4","babozazo","b4b0z4z0",
            "bultero","bult3r0","bultera","bult3r4","bultoso","bult0s0",
            "aceitoso","4c31t0s0","aceitosa","4c31t0s4",
            "desacatado","d3s4c4t4d0","desacatao","d3s4c4t40","desacatá","d3s4c4t4",
            "barraco","b4rr4c0","barraca","b4rr4c4","barraci","b4rr4c1",
            "chivato","ch1v4t0","chivata","ch1v4t4","chivataso","ch1v4t4z0","chivatazo",

            // ── Orientación/discriminación ──
            "pajaro","p4j4r0","pajarito","p4j4r1t0","pajarazo","p4j4r4z0",
            "maricon","m4r1c0n","mariconazo","m4r1c0n4z0","marik","m4r1k",
            "marica","m4r1c4","mariconada","m4r1c0n4d4",
            "cundango","cvnd4ng0","cundanga",
            "bugarron","bvg4rr0n","bugarrona",

            // ── Amenazas ──
            "matate","m4t4t3","m_a_t_a_t_e","m.a.t.a.t.e",
            "suicidate","sv1c1d4t3","s.u.i.c.i.d.a.t.e",
            "te_voy_a_matar","te_vamos_a_dar","d4rt3_p4_b4j0","darte_pa_bajo","darte_pabajo",
            "t3_v0y_4_m4t4r","te_voy_a_explotar","t3_v0y_4_3xpl0t4r",
            "te_rompo_la_boca","t3_r0mp0_l4_b0c4","te_parto_el_coco","t3_p4rt0_3l_c0c0",
            "muerete","mv3r3t3","m.u.e.r.e.t.e",

            // ── Bullying ──
            "salta_pa_atras","salta_patras","s4lt4_p4tr4s",
            "nadie_te_quiere","n4d13_t3_qv13r3","das_asco","d4s_4sc0",
            "hijo_del_diablo","h1j0_d3l_d14bl0",

            // ── Insultos familiares ──
            "mama_gallo","m4m4_g4ll0","mamagallo",
            "mal_nacido","m4l_n4c1d0","malparido","m4lp4r1d0","malparida","m4lp4r1d4",
            "hijo_de_perra","h1j0_d3_p3rr4","hijue_perra",
            "singa_tu_madre","s1ng4_tv_m4dr3","rapa_tu_madre","r4p4_tv_m4dr3",
            "me_cago_en_ti","m3_c4g0_3n_t1","cago_en_tu_madre","c4g0_3n_tv_m4dr3",
            "tu_maldita_madre","tv_m4ld1t4_m4dr3","tu_maldita_mai","tv_m4ld1t4_m41",
            "tu_mardita_madre","tv_m4rd1t4_m4dr3","tu_mardita_mai","tv_m4rd1t4_m41",
            "la_concha_de_tu_madre","l4_c0nch4_d3_tv_m4dr3","concha_tu_madre","c0nch4_tv_m4dr3",
            "ctm","c_t_m","ptm","p_t_m",

            // ── Direcciones ofensivas ──
            "vete_al_diablo","v3t3_4l_d14bl0","vete_a_la_porra","v3t3_4_l4_p0rr4",
            "anda_pal_carajo","4nd4_p4l_c4r4j0","vete_al_carajo","v3t3_4l_c4r4j0",
            "carajo","c4r4j0","carajito","c4r4j1t0","carajita","c4r4j1t4",

            // ── Insultos físicos/apariencia ──
            "perro","p3rr0","perra","p3rr4","perrasa","p3rr4s0","perrazo",
            "perra_sucia","p3rr4_svc14",
            "sucia","svc14","sucio","svc10",
            "asqueroso","4sqv3r0s0","asquerosa","4sqv3r0s4","askeroso","4sk3r0s0",

            // ── Comentarios racistas ──
            "maldito_haitiano","m4ld1t0_h41t14n0","negro_sucio","n3gr0_svc10",
            "mono","m0n0","mona","m0n4","macaco","m4c4c0",

            // ── Insultos de servilismo ──
            "chuchumeco","chvchvm3c0","lambeojo","l4mb30j0","lambe_ojo",
            "rompe_grupo","r0mp3_grvp0","tumba_polvo","tvmb4_p0lv0","tumbapolvo",
            "lambiscon","l4mb1sc0n","lameculo","l4m3cvl0","lameculos",
            "mamaculo","m4m4cvl0","mama_culo",
            "chupa_media","chvp4_m3d14","chupamedias",

            // ── Insultos sexuales ──
            "singa_fiao","s1ng4_f140","rapa_fiao","r4p4_f140",
            "singa_a_veces","s1ng4_4_v3c3s",
            "puto","pvt0","puta","pvt4","putita","pvt1t4","puton","pvt0n",
            "putazo","pvt4z0","prostituta","pr0st1tvt4","ramera","r4m3r4",
            "zorra","z0rr4","z0rrit4",
            "grillo","gr1ll0","grillera","gr1ll3r4",
            "cuera_vieja","cv3r4_v13j4","mujerzuela","mvj3rzv3l4","cualquiera","cv4lqv13r4",

            // ── Insultos económicos ──
            "muerto_de_hambre","mv3rt0_d3_h4mbr3","muerto_jambre","mv3rt0_j4mbr3",
            "arrancao","4rr4nc40","arrancada","4rr4nc4d4",
            "en_olla","3n_0ll4","sin_ni_uno","s1n_n1_vn0",
            "lambe_plato","l4mb3_pl4t0",
            "come_sopa","c0m3_s0p4",

            // ── Insultos de inteligencia ──
            "bruto","brvt0","bruta","brvt4","bestia","b3st14",
            "burro","bvrro","burra","bvrr4","salvaje","s4lv4j3",
            "ignorante","1gn0r4nt3","analfabeto","4n4lf4b3t0","analfabeta","4n4lf4b3t4",
            "tarao","t4r40","tarado","t4r4d0","tarada","t4r4d4",
            "retrasao","r3tr4s40","retrasado","r3tr4s4d0","retrasada","r3tr4s4d4",
            "idiota","1d10t4","imbecil","1mb3c1l",
            "estupido","3stvp1d0","estupida","3stvp1d4",
            "demente","d3m3nt3",

            // ── Carácter/personalidad ──
            "sinverguenza","s1nv3rgv3nz4",
            "vagabundo","v4g4bvnd0","vagabunda","v4g4bvnd4","bagabundo","b4g4bvnd0",
            "desgraciado","d3sgr4c14d0","desgraciada","d3sgr4c14d4","desgraciao","d3sgr4c140",
            "descarado","d3sc4r4d0","descarada","d3sc4r4d4","descarao","d3sc4r40",
            "cornudo","c0rnvd0","cornuda","c0rnvd4","pega_cuerno","p3g4_cv3rn0",
            "arrastrado","4rr4str4d0","arrastrada","4rr4str4d4","arrastrao","4rr4str40",
            "charlatan","ch4rl4t4n","charlatana","ch4rl4t4n4",
            "hipocrita","h1p0cr1t4","falso","f4ls0","falsa","f4ls4",
            "traidor","tr41d0r","traidora","tr41d0r4",

            // ── Basura/escoria ──
            "basura","b4svr4","escoria","3sc0r14",
            "mierdero","m13rd3r0","mierdera","m13rd3r4",
            "come_mierda","c0m3_m13rd4","comemierda","c0m3m13rd4",
            "habla_mierda","h4bl4_m13rd4","hablamierda",
            "jabla_mierda","j4bl4_m13rd4",
            "joputa","j0pvt4",

            // ── Delincuencia ──
            "tiguere","t1gv3r3","tiguera","t1gv3r4","tíguere","tiguerazo","t1gv3r4z0",
            "delincuente","d3l1ncv3nt3",
            "ladron","l4dr0n","ladrona","l4dr0n4","ratero","r4t3r0","ratera","r4t3r4",

            // ── Otros comunes ──
            "inutil","1nvt1l","fracasado","fr4c4s4d0","fracasada","fr4c4s4d4",
            "perdedor","p3rd3d0r","perdedora","p3rd3d0r4","mediocre","m3d10cr3",
            "envidioso","3nv1d10s0","envidiosa","3nv1d10s4",
            "odioso","0d10s0","odiosa","0d10s4",
            "mantenido","m4nt3n1d0","mantenida","m4nt3n1d4","mantenio","m4nt3n10",
            "chulo","chvl0","chula","chvl4","proxeneta","pr0x3n3t4",

            // ── Abuso/violencia ──
            "pedofilo","p3d0f1l0","violador","v10l4d0r","abusador","4bvs4d0r","abusadora","4bvs4d0r4",

            // ── Cara de / cabeza de ──
            "care_tuerca","c4r3_tv3rc4","cara_de_chele","c4r4_d3_ch3l3","care_chele","c4r3_ch3l3",
            "cabeza_de_guevo","c4b3z4_d3_gv3v0","cabezadeguevo","c4b3z4_d3_w3b0",
            "care_guevo","c4r3_gv3v0","cara_de_guevo","c4r4_d3_gv3v0",
            "cara_de_pene","c4r4_d3_p3n3","care_pene","c4r3_p3n3",
            "cabeza_de_ñame","c4b3z4_d3_ñ4m3","care_ñame","c4r3_ñ4m3",

            // ── Creta ──
            "creta","cr3t4","cretaso","cr3t4s0","huevazo","hv3v4z0","huevaso","hv3v4s0",

            // ── Frases completas ──
            "tu_madre_en_tanga","tv_m4dr3_3n_t4ng4",
            "tu_abuela_en_patineta","tv_4bv3l4_3n_p4t1n3t4",
            "anda_a_freir_tusa","4nd4_4_fr31r_tvs4",
            "mosquita_muerta","m0sqv1t4_mv3rt4",
            "buen_mmg","bv3n_mmg","buen_mierda","bv3n_m13rd4",
            "doble_cara","d0bl3_c4r4",
            "bueno_para_nada","bv3n0_p4r4_n4d4",
            "sangre_pesada","s4ngr3_p3s4d4",
            "vende_patria","v3nd3_p4tr14",
            "roba_marido","r0b4_m4r1d0","quita_marido","qv1t4_m4r1d0",
            "saca_cuarto","s4c4_cv4rt0",
            "busca_pleito","bvsc4_pl31t0",

            // ── Leetspeak extendidas (duplicados con trailing 0/44 del user) ──
            "m4m4guev0","m4m4gv3b0","m4m4hu3v0","m4m4hu3b0","m4m4gv",
            "cooooño","coooño","c000ñ0","c00ñ0","c000n0","c00n0",
            "piti","p1t1","pitis","congo","c0ng0","maco","m4c0",
            "brecha","br3ch4","brechero","br3ch3r0","brechera",
            "zangan","z4ng4n","zangana","z4ng4n4","zangano","z4ng4n0",
            "soplon","s0pl0n","bocina","b0c1n4",
            "loco","l0c0","loca","l0c4","loquito","l0qv1t0","loquita","l0qv1t4",
            "pendejo","pendeja","cabron","cabrón","cabrona","culo","joder","jodido","jodida",
            "bastardo","bastarda","hijodeputa"
        };

        // ══════════════════════════════════════════════════════════════
        // REGEX DINÁMICO — compilado a partir del HashSet
        // Usa \b para word boundaries, pero permite puntuación adyacente
        // ══════════════════════════════════════════════════════════════
        private static readonly Regex _regexProfanidad;

        static ProfanityService()
        {
            // Ordenar por longitud descendente para que frases largas matcheen primero
            var palabras = _diccionarioLocal
                .OrderByDescending(p => p.Length)
                .Select(p => Regex.Escape(p).Replace("\\_", "[_ ]")) // Permitir espacio o underscore
                .ToList();

            var patron = string.Join("|", palabras);

            _regexProfanidad = new Regex(
                $@"(?<!\w)({patron})(?!\w)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                TimeSpan.FromSeconds(3)
            );
        }

        public ProfanityService(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ProfanityService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        // ══════════════════════════════════════════════════════════════
        // PIPELINE PRINCIPAL
        // ══════════════════════════════════════════════════════════════
        public async Task<ProfanityResult> ValidarYCensurarAsync(string texto, int usuarioId)
        {
            var resultado = new ProfanityResult
            {
                TextoOriginal = texto,
                TextoCensurado = texto,
                FueCensurado = false
            };

            if (string.IsNullOrWhiteSpace(texto))
                return resultado;

            // ┌─────────────────────────────────────────────┐
            // │ PASO 0: Normalización del texto entrante    │
            // │  → ToLowerInvariant + Eliminar diacríticos  │
            // └─────────────────────────────────────────────┘
            var textoNormalizado = RemoverDiacriticos(texto.ToLowerInvariant());

            // ┌─────────────────────────────────────────────┐
            // │ CAPA 1: Diccionario local (Regex + HashSet) │
            // └─────────────────────────────────────────────┘
            var matches = _regexProfanidad.Matches(textoNormalizado);
            if (matches.Count > 0)
            {
                resultado.FueCensurado = true;
                foreach (Match m in matches)
                {
                    var palabra = m.Value.ToLowerInvariant();
                    if (!resultado.PalabrasDetectadas.Contains(palabra))
                        resultado.PalabrasDetectadas.Add(palabra);
                }

                // Censurar sobre el texto ORIGINAL preservando posiciones
                // Usamos el texto normalizado para encontrar posiciones, 
                // luego reemplazamos en el original
                resultado.TextoCensurado = _regexProfanidad.Replace(
                    texto, // Operar sobre el original para preservar casing del resto
                    match => new string('*', match.Value.Length));

                _logger.LogWarning(
                    "[Profanity-Local] Usuario {UsuarioId} — {Count} palabra(s) censurada(s): {Palabras}",
                    usuarioId, matches.Count, string.Join(", ", resultado.PalabrasDetectadas));
            }

            // ┌─────────────────────────────────────────────┐
            // │ CAPA 2: API Externa — PurgoMalum (EN/ES)    │
            // └─────────────────────────────────────────────┘
            var apiExternaDetecto = await ConsultarPurgoMalumAsync(resultado.TextoCensurado);

            if (apiExternaDetecto && !resultado.FueCensurado)
            {
                resultado.FueCensurado = true;
                resultado.PalabrasDetectadas.Add("[API-PurgoMalum]");

                _logger.LogWarning(
                    "[Profanity-API] Usuario {UsuarioId} — texto flaggeado por PurgoMalum",
                    usuarioId);
            }

            // ┌──────────────────────────────────────────────┐
            // │ PENALIZACIÓN: -5 pts si se censuró algo      │
            // └──────────────────────────────────────────────┘
            if (resultado.FueCensurado)
            {
                await PenalizarUsuarioAsync(usuarioId, 5,
                    $"Lenguaje ofensivo detectado: {string.Join(", ", resultado.PalabrasDetectadas)}");
            }

            return resultado;
        }

        // ══════════════════════════════════════════════════════════════
        // NORMALIZACIÓN: Remover acentos y diacríticos
        // "azaroso" == "azaróso" == "ázárósó" después de esto
        // ══════════════════════════════════════════════════════════════
        private static string RemoverDiacriticos(string texto)
        {
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalizado.Length);

            foreach (var c in normalizado)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
                if (categoria != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // ══════════════════════════════════════════════════════════════
        // CAPA 2: PurgoMalum API — Consulta gratuita EN/ES
        // https://www.purgomalum.com/service/containsprofanity?text=...
        // Retorna "true" o "false" como plain text
        // ══════════════════════════════════════════════════════════════
        private async Task<bool> ConsultarPurgoMalumAsync(string texto)
        {
            // Si hay una API custom configurada, úsala en su lugar
            var customApiUrl = _configuration["ModerationSettings:ApiUrl"];
            if (!string.IsNullOrEmpty(customApiUrl))
            {
                return await ConsultarApiCustomAsync(customApiUrl, texto);
            }

            // Default: PurgoMalum (gratuita, sin API key)
            try
            {
                var client = _httpClientFactory.CreateClient("ModerationApi");
                var encoded = Uri.EscapeDataString(texto);
                var url = $"https://www.purgomalum.com/service/containsprofanity?text={encoded}";

                var response = await client.GetStringAsync(url);
                return bool.TryParse(response.Trim(), out var flagged) && flagged;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Profanity-PurgoMalum] Error al consultar API. Se continúa solo con filtro local.");
                return false;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // API CUSTOM — Endpoint configurable (preparado para Perspective API)
        // ══════════════════════════════════════════════════════════════
        private async Task<bool> ConsultarApiCustomAsync(string apiUrl, string texto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ModerationApi");
                var apiKey = _configuration["ModerationSettings:ApiKey"] ?? "";

                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = JsonContent.Create(new { text = texto })
                };

                if (!string.IsNullOrEmpty(apiKey))
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(json);

                    // Soporta múltiples formatos de respuesta
                    if (doc.RootElement.TryGetProperty("flagged", out var flagProp))
                        return flagProp.GetBoolean();
                    if (doc.RootElement.TryGetProperty("isToxic", out var toxicProp))
                        return toxicProp.GetBoolean();
                    // Perspective API format
                    if (doc.RootElement.TryGetProperty("attributeScores", out var scores))
                    {
                        if (scores.TryGetProperty("TOXICITY", out var toxicity))
                        {
                            var score = toxicity.GetProperty("summaryScore").GetProperty("value").GetDouble();
                            return score >= 0.7; // Umbral: 70% de toxicidad
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Profanity-CustomAPI] Error al consultar API externa");
            }

            return false;
        }

        // ══════════════════════════════════════════════════════════════
        // PENALIZACIÓN
        // ══════════════════════════════════════════════════════════════
        private async Task PenalizarUsuarioAsync(int usuarioId, int puntosARestar, string motivo)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return;

            usuario.PuntosReputacion = Math.Max(0, usuario.PuntosReputacion - puntosARestar);
            usuario.Reputacion = CalcularNivel(usuario.PuntosReputacion);
            usuario.MotivoReputacion = motivo;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "[Reputación] Usuario {Id} penalizado -{Puntos}pts → {NuevoPuntaje} ({Nivel}). Motivo: {Motivo}",
                usuarioId, puntosARestar, usuario.PuntosReputacion, usuario.Reputacion, motivo);
        }

        /// <summary>
        /// Calcula el NivelReputacion basado en los puntos actuales.
        /// </summary>
        public static NivelReputacion CalcularNivel(int puntos)
        {
            return puntos switch
            {
                > 80 => NivelReputacion.Excelente,
                > 60 => NivelReputacion.Buena,
                > 40 => NivelReputacion.Regular,
                > 20 => NivelReputacion.Mala,
                _    => NivelReputacion.Critica
            };
        }
    }
}
