let veiculoSelecionado = null;
let usuarioAutenticado = null; // Guarda { nome, email, role, token }
// =========================================================================
//  GERENCIAMENTO DE AUTENTICAÇÃO (JWT & RBAC)
// =========================================================================
function verificarSessao() {
  const salvo = localStorage.getItem('fleetlog_user');
  if (salvo) {
    try {
      usuarioAutenticado = JSON.parse(salvo);
    } catch {
      usuarioAutenticado = null;
    }
  }
  atualizarNavbarAuth();
}
function atualizarNavbarAuth() {
  const display = document.getElementById('user-display');
  const btnAuth = document.getElementById('btn-auth-action');
  if (usuarioAutenticado && usuarioAutenticado.token) {
    const roleBadge = usuarioAutenticado.role === 'Admin' 
      ? '<span class="badge badge-role-admin">👑 Admin</span>' 
      : '<span class="badge badge-role-user">👤 Operador (Usuário)</span>';
    
    display.innerHTML = `Logado como: <strong>${usuarioAutenticado.nome}</strong> ${roleBadge}`;
    btnAuth.textContent = 'Sair (Logout)';
    btnAuth.className = 'btn-logout-nav';
    btnAuth.onclick = executarLogout;
  } else {
    display.innerHTML = '🌐 Operando em modo público (Apenas Leitura)';
    btnAuth.textContent = 'Entrar (JWT)';
    btnAuth.className = 'btn-login-nav';
    btnAuth.onclick = abrirModalLogin;
  }
}
function abrirModalLogin() {
  document.getElementById('login-error').classList.add('hidden');
  document.getElementById('modal-login').classList.remove('hidden');
}
function fecharModalLogin() {
  document.getElementById('modal-login').classList.add('hidden');
}
function preencherLogin(email, senha) {
  document.getElementById('login-email').value = email;
  document.getElementById('login-senha').value = senha;
}
async function executarLogin(evento) {
  evento.preventDefault();
  const erroBox = document.getElementById('login-error');
  erroBox.classList.add('hidden');
  const payload = {
    email: document.getElementById('login-email').value,
    senha: document.getElementById('login-senha').value
  };
  try {
    const resposta = await fetch('/api/Auth/Login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const dados = await resposta.json();
    if (!resposta.ok) {
      throw new Error(dados.mensagem || 'Falha ao autenticar.');
    }
    // Salva sessão
    usuarioAutenticado = {
      token: dados.token,
      nome: dados.usuario.nome,
      email: dados.usuario.email,
      role: dados.usuario.role
    };
    localStorage.setItem('fleetlog_user', JSON.stringify(usuarioAutenticado));
    fecharModalLogin();
    atualizarNavbarAuth();
    exibirAlerta(`Bem-vindo, ${usuarioAutenticado.nome}! Perfil ativo: ${usuarioAutenticado.role}.`, 'success');
  } catch (erro) {
    erroBox.textContent = erro.message;
    erroBox.classList.remove('hidden');
  }
}
function executarLogout() {
  usuarioAutenticado = null;
  localStorage.removeItem('fleetlog_user');
  atualizarNavbarAuth();
  exibirAlerta('Você encerrou a sessão. O sistema voltou ao modo público.', 'success');
}
function obterHeadersAutenticados() {
  const headers = { 'Content-Type': 'application/json' };
  if (usuarioAutenticado && usuarioAutenticado.token) {
    headers['Authorization'] = `Bearer ${usuarioAutenticado.token}`;
  }
  return headers;
}
// =========================================================================
//  NAVEGAÇÃO ENTRE TELAS (SEM RELOAD)
// =========================================================================
function mostrarTela(idTela) {
  document.getElementById('view-lista').classList.add('hidden');
  document.getElementById('view-detalhe').classList.add('hidden');
  document.getElementById('view-formulario').classList.add('hidden');
  document.getElementById(idTela).classList.remove('hidden');
  limparAlerta();
}
function voltarParaLista() {
  veiculoSelecionado = null;
  carregarVeiculos();
  mostrarTela('view-lista');
}
function voltarParaDetalhe() {
  if (veiculoSelecionado) {
    abrirDetalhesVeiculo(veiculoSelecionado.id);
  } else {
    voltarParaLista();
  }
}
function exibirAlerta(mensagem, tipo = 'error') {
  const box = document.getElementById('alert-box');
  box.textContent = mensagem;
  box.className = `alert-box alert-${tipo}`;
  box.classList.remove('hidden');
  window.scrollTo({ top: 0, behavior: 'smooth' });
}
function limparAlerta() {
  document.getElementById('alert-box').classList.add('hidden');
}
// =========================================================================
//  VISÃO 1: LISTAR VEÍCULOS (FETCH GET)
// =========================================================================
async function carregarVeiculos() {
  const grid = document.getElementById('grid-veiculos');
  grid.innerHTML = '<p class="loading">Carregando frota...</p>';
  try {
    const resposta = await fetch('/api/veiculos');
    if (!resposta.ok) throw new Error('Falha ao consultar veículos da frota.');
    const veiculos = await resposta.json();
    if (veiculos.length === 0) {
      grid.innerHTML = '<p>Nenhum veículo cadastrado na frota.</p>';
      return;
    }
    grid.innerHTML = veiculos.map(v => `
      <div class="card-veiculo" onclick="abrirDetalhesVeiculo('${v.id}')">
        <div class="flex-between">
          <h3>${v.placa}</h3>
          <span class="badge ${obterClasseBadge(v.statusOperacional)}">${formatarStatus(v.statusOperacional)}</span>
        </div>
        <p><strong>Modelo:</strong> ${v.modelo}</p>
        <p><strong>Capacidade Máxima:</strong> ${v.capacidadeCargaKg} kg</p>
        <p><strong>Ano:</strong> ${v.anoFabricacao}</p>
      </div>
    `).join('');
  } catch (erro) {
    exibirAlerta(erro.message, 'error');
  }
}
function formatarStatus(status) {
  if (status === 1 || status === 'Disponivel') return 'Disponível';
  if (status === 2 || status === 'EmViagem') return 'Em Viagem';
  if (status === 3 || status === 'EmManutencao') return 'Em Manutenção';
  if (status === 4 || status === 'Inativo') return 'Inativo';
  return status;
}
function obterClasseBadge(status) {
  if (status === 1 || status === 'Disponivel') return 'badge-disponivel';
  if (status === 3 || status === 'EmManutencao') return 'badge-manutencao';
  return 'badge-viagem';
}
// =========================================================================
//  VISÃO 2: DETALHES DO VEÍCULO & SUAS CARGAS (FETCH GET)
// =========================================================================
async function abrirDetalhesVeiculo(id) {
  limparAlerta();
  try {
    const respVeiculo = await fetch(`/api/veiculos/${id}`);
    if (!respVeiculo.ok) throw new Error('Veículo não encontrado.');
    veiculoSelecionado = await respVeiculo.json();
    const respCargas = await fetch(`/api/ordens-carga?veiculoId=${id}`);
    const cargas = respCargas.ok ? await respCargas.json() : [];
    const pesoTotal = cargas
      .filter(c => c.statusEntrega === 'Pendente' || c.statusEntrega === 'EmTransito')
      .reduce((acc, c) => acc + c.pesoCargaKg, 0);
    const infoCard = document.getElementById('detalhe-veiculo-info');
    infoCard.innerHTML = `
      <h2>Veículo: ${veiculoSelecionado.placa} (${veiculoSelecionado.modelo})</h2>
      <p><strong>Status Operacional:</strong> <span class="badge ${obterClasseBadge(veiculoSelecionado.statusOperacional)}">${formatarStatus(veiculoSelecionado.statusOperacional)}</span></p>
      <p><strong>Capacidade Máxima:</strong> ${veiculoSelecionado.capacidadeCargaKg} kg</p>
      <p><strong>Carga Atual Alocada:</strong> ${pesoTotal} kg (${veiculoSelecionado.capacidadeCargaKg - pesoTotal} kg disponíveis)</p>
    `;
    const tbody = document.getElementById('tbody-cargas');
    if (cargas.length === 0) {
      tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Nenhuma carga despachada para este veículo.</td></tr>';
    } else {
      tbody.innerHTML = cargas.map(c => `
        <tr>
          <td><strong>${c.codigoRastreio}</strong></td>
          <td>${c.destinoCidade}</td>
          <td>${c.pesoCargaKg} kg</td>
          <td>R$ ${c.valorFrete.toFixed(2)}</td>
          <td><span class="badge">${c.statusEntrega}</span></td>
          <td>
            ${c.statusEntrega !== 'Entregue' 
                 ? `<button class="btn-primary" onclick="concluirEntrega('${c.id}')">Concluir Entrega</button>` 
                 : '—'}
          </td>
        </tr>
      `).join('');
    }
    mostrarTela('view-detalhe');
  } catch (erro) {
    exibirAlerta(erro.message, 'error');
  }
}
// Exclusão de Veículo (DELETE com RBAC restrito a Admin)
async function confirmarExclusaoVeiculo() {
  if (!veiculoSelecionado) return;
  // Verificação de permissão no frontend
  if (!usuarioAutenticado || !usuarioAutenticado.token) {
    exibirAlerta('Acesso Negado (401)! Você precisa estar logado como Administrador para excluir veículos.', 'error');
    abrirModalLogin();
    return;
  }
  if (usuarioAutenticado.role !== 'Admin') {
    exibirAlerta(`Acesso Negado (403 Forbidden)! Seu perfil atual é "${usuarioAutenticado.role}". Apenas o perfil Administrador pode excluir veículos (Regra RBAC).`, 'error');
    return;
  }
  if (!confirm(`Tem certeza que deseja excluir o veículo ${veiculoSelecionado.placa}? Esta ação chamará DELETE /api/Veiculos/{id} com o token do Admin.`)) {
    return;
  }
  try {
    const resposta = await fetch(`/api/veiculos/${veiculoSelecionado.id}`, {
      method: 'DELETE',
      headers: obterHeadersAutenticados()
    });
    if (resposta.status === 204) {
      exibirAlerta(`Veículo ${veiculoSelecionado.placa} excluído com sucesso (Status 204 No Content)!`, 'success');
      voltarParaLista();
    } else if (resposta.status === 403) {
      throw new Error('403 Forbidden: Usuário não tem permissão de Admin.');
    } else {
      throw new Error('Falha ao excluir o veículo.');
    }
  } catch (erro) {
    exibirAlerta(erro.message, 'error');
  }
}
// =========================================================================
//  VISÃO 3: FORMULÁRIO DE NOVA CARGA (FETCH POST COM CAPTURA DE 400)
// =========================================================================
function mostrarFormularioCarga() {
  if (!usuarioAutenticado || !usuarioAutenticado.token) {
    exibirAlerta('Acesso Restrito (401)! Você precisa estar logado como Operador ou Administrador para despachar cargas.', 'error');
    abrirModalLogin();
    return;
  }
  if (!veiculoSelecionado) return;
  document.getElementById('form-veiculo-id').value = veiculoSelecionado.id;
  document.getElementById('form-veiculo-desc').textContent = 
    `Alocando mercadoria para o caminhão ${veiculoSelecionado.placa} (Capacidade: ${veiculoSelecionado.capacidadeCargaKg} kg).`;
  document.getElementById('form-nova-carga').reset();
  mostrarTela('view-formulario');
}
async function salvarNovaCarga(evento) {
  evento.preventDefault();
  limparAlerta();
   if (!usuarioAutenticado || !usuarioAutenticado.token) {
    exibirAlerta('Acesso Negado (401)! Você precisa estar autenticado como Operador ou Admin para cadastrar cargas.', 'error');
    abrirModalLogin();
    return;
  }
  const payload = {
    veiculoId: document.getElementById('form-veiculo-id').value,
    codigoRastreio: document.getElementById('form-codigo').value,
    destinoCidade: document.getElementById('form-destino').value,
    pesoCargaKg: parseFloat(document.getElementById('form-peso').value),
    valorFrete: parseFloat(document.getElementById('form-valor').value)
  };
  try {
    const resposta = await fetch('/api/ordens-carga', {
      method: 'POST',
      headers: obterHeadersAutenticados(),
      body: JSON.stringify(payload)
    });
    const resultado = await resposta.json();
    if (!resposta.ok) {
      // Captura a violação da regra de negócio (Status 400 Bad Request)
      throw new Error(resultado.mensagem || 'Falha ao despachar a carga.');
    }
    exibirAlerta('Ordem de carga despachada com sucesso (Status 201 Created)!', 'success');
    abrirDetalhesVeiculo(veiculoSelecionado.id);
  } catch (erro) {
    exibirAlerta(erro.message, 'error');
  }
}
// Concluir Entrega (PUT)
async function concluirEntrega(cargaId) {
  if (!usuarioAutenticado || !usuarioAutenticado.token) {
    exibirAlerta('Acesso Restrito (401)! Você precisa estar logado para concluir entregas.', 'error');
    abrirModalLogin();
    return;
  }
  try {
    const resp = await fetch(`/api/ordens-carga/${cargaId}/status`, {
      method: 'PUT',
      headers: obterHeadersAutenticados(),
      body: JSON.stringify(3) // StatusEntrega.Entregue = 3
    });
    if (!resp.ok) throw new Error('Não foi possível atualizar o status da entrega.');
    exibirAlerta('Entrega concluída com sucesso! Capacidade do veículo liberada.', 'success');
    abrirDetalhesVeiculo(veiculoSelecionado.id);
  } catch (erro) {
    exibirAlerta(erro.message, 'error');
  }
}
// Inicialização
document.addEventListener('DOMContentLoaded', () => {
  verificarSessao();
  carregarVeiculos();
});