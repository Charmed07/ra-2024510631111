let veiculoSelecionado = null;
let usuarioAutenticado = null; // Guarda { nome, email, role, token }
// =========================================================================
//  GERENCIAMENTO DE AUTENTICAÇÃO
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
      ? '<span class="badge badge-role-admin">👑 Administrador</span>' 
      : '<span class="badge badge-role-user">👤 Operador</span>';
    
    display.innerHTML = `Olá, <strong>${usuarioAutenticado.nome}</strong> (${roleBadge})`;
    btnAuth.textContent = 'Sair';
    btnAuth.className = 'btn-logout-nav';
    btnAuth.onclick = executarLogout;
  } else {
    display.innerHTML = '🌐 Modo Consulta (Apenas Leitura)';
    btnAuth.textContent = 'Entrar';
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
      throw new Error(dados.mensagem || 'E-mail ou senha incorretos.');
    }
    usuarioAutenticado = {
      token: dados.token,
      nome: dados.usuario.nome,
      email: dados.usuario.email,
      role: dados.usuario.role
    };
    localStorage.setItem('fleetlog_user', JSON.stringify(usuarioAutenticado));
    fecharModalLogin();
    atualizarNavbarAuth();
    exibirAlerta(`Login realizado com sucesso! Bem-vindo(a), ${usuarioAutenticado.nome}.`, 'success');
  } catch (erro) {
    erroBox.textContent = erro.message;
    erroBox.classList.remove('hidden');
  }
}
function executarLogout() {
  usuarioAutenticado = null;
  localStorage.removeItem('fleetlog_user');
  atualizarNavbarAuth();
  exibirAlerta('Sessão encerrada com sucesso. O sistema voltou ao modo de consulta.', 'success');
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
    if (!resposta.ok) throw new Error('Não foi possível carregar os veículos. Verifique a conexão com o servidor.');
    const veiculos = await resposta.json();
    if (veiculos.length === 0) {
      grid.innerHTML = '<p>Nenhum veículo cadastrado na frota no momento.</p>';
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
// Exclusão de Veículo (Restrito a Administrador)
async function confirmarExclusaoVeiculo() {
  if (!veiculoSelecionado) return;
  if (!usuarioAutenticado || !usuarioAutenticado.token) {
    exibirAlerta('Você precisa fazer login como Administrador para poder excluir veículos.', 'error');
    abrirModalLogin();
    return;
  }
  if (usuarioAutenticado.role !== 'Admin') {
    exibirAlerta('Ação não permitida: apenas Administradores têm permissão para remover veículos da frota.', 'error');
    return;
  }
  if (!confirm(`Deseja realmente remover o veículo ${veiculoSelecionado.placa} da frota? Esta ação não pode ser desfeita.`)) {
    return;
  }
  try {
    const resposta = await fetch(`/api/veiculos/${veiculoSelecionado.id}`, {
      method: 'DELETE',
      headers: obterHeadersAutenticados()
    });
    if (resposta.status === 204) {
      exibirAlerta(`Veículo ${veiculoSelecionado.placa} removido da frota com sucesso!`, 'success');
      voltarParaLista();
    } else if (resposta.status === 403) {
      throw new Error('Permissão insuficiente para excluir este veículo.');
    } else {
      throw new Error('Não foi possível excluir o veículo no momento.');
    }
  } catch (erro) {
    exibirAlerta(erro.message, 'error');
  }
}
// =========================================================================
//  VISÃO 3: FORMULÁRIO DE NOVA CARGA
// =========================================================================
function mostrarFormularioCarga() {
  if (!usuarioAutenticado || !usuarioAutenticado.token) {
    exibirAlerta('Você precisa fazer login para despachar novas cargas.', 'error');
    abrirModalLogin();
    return;
  }
  if (!veiculoSelecionado) return;
  document.getElementById('form-veiculo-id').value = veiculoSelecionado.id;
  document.getElementById('form-veiculo-desc').textContent = 
    `Alocando mercadoria para o caminhão ${veiculoSelecionado.placa} (Capacidade máxima: ${veiculoSelecionado.capacidadeCargaKg} kg).`;
  document.getElementById('form-nova-carga').reset();
  mostrarTela('view-formulario');
}
async function salvarNovaCarga(evento) {
  evento.preventDefault();
  limparAlerta();
  if (!usuarioAutenticado || !usuarioAutenticado.token) {
    exibirAlerta('Você precisa fazer login para despachar cargas.', 'error');
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
      // Exibe a mensagem de negócio direta (ex: "O peso total excede a capacidade do veículo")
      throw new Error(resultado.mensagem || 'Não foi possível despachar a carga.');
    }
    exibirAlerta('Carga despachada com sucesso!', 'success');
    abrirDetalhesVeiculo(veiculoSelecionado.id);
  } catch (erro) {
    exibirAlerta(erro.message, 'error');
  }
}
// Concluir Entrega
async function concluirEntrega(cargaId) {
  if (!usuarioAutenticado || !usuarioAutenticado.token) {
    exibirAlerta('Você precisa fazer login para concluir entregas.', 'error');
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
    exibirAlerta('Entrega concluída com sucesso! A capacidade do veículo foi liberada.', 'success');
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