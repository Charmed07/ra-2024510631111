# HANDOUT — AULA 09

## Projete a Matriz de Acesso

*SisAcad — sistema acadêmico fictício — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Vocês são o time de segurança contratado para definir QUEM pode fazer O QUÊ no SisAcad, o novo sistema acadêmico da faculdade. Os papéis já existem: Aluno, Professor, Coordenador e Secretaria.

- Preencham a matriz abaixo com ✓ (pode) ou ✗ (não pode) para cada papel × ação
- Justifiquem por escrito os 3 casos que geraram mais discussão na dupla
- Respondam à pergunta-armadilha da Parte 03 — ela esconde o limite do modelo

*⏱️ Tempo: 25 minutos  |  👥 Formato: em duplas  |  Regra de ouro: na dúvida, NEGUE o acesso.*

> **Nomes:** João Antonio   **Turma:** ____________________   **Data:** 15 / 10 / 2026

## PARTE 01 — A matriz papel × permissão

Marquem ✓ (pode) ou ✗ (não pode) em cada célula:

| **Ação** | **Aluno** | **Professor** | **Coordenador** | **Secretaria** |
| --- | --- | --- | --- | --- |
| 1. Ver as próprias notas | ✓ | ✗ | ✗ | ✗ |
| 2. Ver notas de toda a turma | ✗ | ✓ | ✓ | ✓ |
| 3. Lançar nota | ✗ | ✓ | ✗ | ✗ |
| 4. Alterar nota já lançada | ✗ | ✓ | ✓ | ✗ |
| 5. Matricular aluno em disciplina | ✗ | ✗ | ✓ | ✓ |
| 6. Trancar matrícula | ✗ | ✗ | ✗ | ✓ |
| 7. Criar nova disciplina | ✗ | ✗ | ✓ | ✗ |
| 8. Ver dados pessoais de aluno | ✓ | ✗ | ✗ | ✓ |
| 9. Emitir histórico escolar | ✗ | ✗ | ✗ | ✓ |
| 10. Aprovar colação de grau | ✗ | ✗ | ✓ | ✓ |

*Dica: pensem em consequências — quem responde perante o MEC se uma nota for alterada indevidamente? E perante a LGPD se dados pessoais vazarem?*

## PARTE 02 — Os casos polêmicos

Quais 3 células geraram mais discussão na dupla? Anotem a célula (ação + papel) e a justificativa da decisão final:

1. Caso 1: Alterar nota já lançada

R: No papel do coordenador, discutimos se deveriam ter essa função, pois ele pode realizar essa ação em casos como, em periodo de fechamento de semestre , onde o professor já realizou o lançamento e não tem mais permissão para modificar, o coordenador poderá com o alinhamento feito com o professor, alterar a nota do aluno em questão.

2. Caso 2: Matricular aluno em disciplina.

R: Papel da sacretaria , discutimos se deveria ter essa função , pois ele deve fazer a grade de calouros e trasferencias.

3. Caso 3: Aprovar colação de grau

R: Papel do coordenador, discutimos se deveria ter essa função , pois ele deve aprovar a colação de grau dos alunos que cumpriram todos os requisitos para a formatura.

## PARTE 03 — A pergunta-armadilha

Vocês marcaram que Professor pode “lançar nota”. Mas… de QUALQUER turma? O professor de Cálculo pode lançar nota na turma de Arquitetura de Aplicações Web?

1. O papel “Professor” sozinho é suficiente para decidir esse acesso? Por quê?

R: Não, pois o professor de Cálculo não pode lançar nota na turma de Arquitetura de Aplicações Web, pois ele não é o professor daquela turma. O sistema precisa conhecer a relação entre o professor e a turma para decidir corretamente.

2. Que informação ALÉM do papel o sistema precisa conhecer para decidir corretamente?

R: O sistema precisa conhecer a relação entre o professor e a turma, ou seja, se o professor está vinculado àquela turma específica para poder lançar notas.

3. Onde vocês já viram esse mesmo problema nesta disciplina? (dica: Aula 07…)

R: Esse mesmo problema foi visto na Aula 07, quando discutimos sobre o controle de acesso baseado em papéis (RBAC) e a necessidade de verificar não apenas o papel do usuário, mas também o contexto específico em que ele está atuando, como a relação entre professor e turma.

## DESAFIO

1. Chegou um pedido da coordenação: criar o papel “Monitor” — um aluno que ajuda o professor em uma disciplina. Que permissões vocês dariam ao Monitor? Que risco novo esse papel cria?

R: O papel de Monitor poderia ter permissões como: ver as notas da disciplina, auxiliar na correção de trabalhos e provas, e ajudar na organização das aulas. No entanto, esse papel cria o risco de vazamento de informações sensíveis, como notas de outros alunos, caso o monitor não seja confiável ou não siga as regras de confidencialidade. Além disso, é importante definir claramente os limites das ações do monitor para evitar conflitos com o professor titular da disciplina.

 O risco de vazamento de informações sensíveis é significativo, pois o monitor terá acesso a dados que não deveriam ser compartilhados com outros alunos. Portanto, é crucial implementar medidas de segurança e monitoramento para garantir que o monitor cumpra suas responsabilidades sem comprometer a privacidade dos demais alunos.
