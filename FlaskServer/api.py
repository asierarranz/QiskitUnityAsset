#!/usr/bin/env python3

from qiskit import QuantumRegister, ClassicalRegister
from qiskit import QuantumCircuit, Aer, execute, IBMQ


def run_qasm(qasm, backend_to_run="qasm_simulator", api_token=None):
    if api_token:
        IBMQ.enable_account(api_token)
    qc = QuantumCircuit.from_qasm_str(qasm)
    backend = Aer.get_backend(backend_to_run)
    job_sim = execute(qc, backend)
    sim_result = job_sim.result()
    return sim_result.get_counts(qc)
