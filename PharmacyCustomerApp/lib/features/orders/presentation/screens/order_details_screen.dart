import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../domain/entities/customer_order.dart';
import '../providers/orders_providers.dart';

class OrderDetailsScreen extends ConsumerWidget {
  const OrderDetailsScreen({required this.orderId, super.key});
  final int orderId;

  Future<void> _performAction(
    BuildContext context,
    WidgetRef ref, {
    required bool cancel,
  }) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: Text(cancel ? 'Cancel this order?' : 'Confirm delivery?'),
        content: Text(
          cancel
              ? 'This will cancel the order and restore its reserved stock.'
              : 'Confirm that the order arrived. The API will mark it complete and paid.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext, false),
            child: const Text('Keep order'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(dialogContext, true),
            child: Text(cancel ? 'Cancel order' : 'Confirm arrival'),
          ),
        ],
      ),
    );
    if (confirmed != true) return;
    try {
      final actions = ref.read(customerOrderActionsProvider);
      if (cancel) {
        await actions.cancel(orderId);
      } else {
        await actions.confirmDelivery(orderId);
      }
      ref.invalidate(orderDetailsProvider(orderId));
      ref.invalidate(ordersProvider);
    } catch (error) {
      if (context.mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text(error.toString())));
      }
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final order = ref.watch(orderDetailsProvider(orderId));
    return Scaffold(
      appBar: AppBar(title: const Text('Order details')),
      body: order.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stack) => Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(error.toString(), textAlign: TextAlign.center),
                const SizedBox(height: 12),
                OutlinedButton(
                  onPressed: () =>
                      ref.invalidate(orderDetailsProvider(orderId)),
                  child: const Text('Try again'),
                ),
              ],
            ),
          ),
        ),
        data: (data) => _OrderDetailContent(
          order: data,
          onCancel: data.canConfirmDelivery
              ? () => _performAction(context, ref, cancel: true)
              : null,
          onConfirmDelivery: data.canConfirmDelivery
              ? () => _performAction(context, ref, cancel: false)
              : null,
        ),
      ),
    );
  }
}

class _OrderDetailContent extends StatelessWidget {
  const _OrderDetailContent({
    required this.order,
    this.onCancel,
    this.onConfirmDelivery,
  });
  final CustomerOrder order;
  final VoidCallback? onCancel;
  final VoidCallback? onConfirmDelivery;

  @override
  Widget build(BuildContext context) => ListView(
    padding: const EdgeInsets.all(20),
    children: [
      Card(
        child: Padding(
          padding: const EdgeInsets.all(18),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                order.orderNumber,
                style: Theme.of(context).textTheme.titleLarge,
              ),
              const SizedBox(height: 10),
              _InfoRow(label: 'Status', value: _statusLabel(order.status)),
              _InfoRow(label: 'Placed', value: _dateLabel(order.orderDate)),
              _InfoRow(
                label: 'Payment',
                value: order.paymentMethod == 0
                    ? 'Cash on delivery'
                    : order.paymentMethod == 1
                    ? 'Credit card'
                    : 'Not specified',
              ),
              _InfoRow(
                label: 'Payment state',
                value: order.isPaid ? 'Paid' : 'Due when delivery is confirmed',
              ),
              if (order.deliveryAddress?.isNotEmpty == true)
                _InfoRow(
                  label: 'Delivery address',
                  value: order.deliveryAddress!,
                ),
              if (order.notes?.isNotEmpty == true)
                _InfoRow(label: 'Notes', value: order.notes!),
            ],
          ),
        ),
      ),
      const SizedBox(height: 20),
      Text('Items', style: Theme.of(context).textTheme.titleLarge),
      const SizedBox(height: 8),
      if (order.items.isEmpty)
        const Card(
          child: Padding(
            padding: EdgeInsets.all(16),
            child: Text('No order items were returned.'),
          ),
        )
      else
        ...order.items.map(
          (item) => Card(
            child: ListTile(
              title: Text(item.productName),
              subtitle: Text(
                '${item.quantity} × ${item.unitPrice.toStringAsFixed(2)}',
              ),
              trailing: Text(item.totalPrice.toStringAsFixed(2)),
            ),
          ),
        ),
      const SizedBox(height: 14),
      Card(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              _InfoRow(
                label: 'Items total',
                value: order.totalAmount.toStringAsFixed(2),
              ),
              _InfoRow(
                label: 'Discount',
                value: order.discount.toStringAsFixed(2),
              ),
              _InfoRow(
                label: 'Delivery',
                value: order.deliveryFees.toStringAsFixed(2),
              ),
              const Divider(),
              _InfoRow(
                label: 'Order total',
                value: order.netAmount.toStringAsFixed(2),
                emphasize: true,
              ),
            ],
          ),
        ),
      ),
      if (onConfirmDelivery != null || onCancel != null) ...[
        const SizedBox(height: 18),
        FilledButton.icon(
          onPressed: onConfirmDelivery,
          icon: const Icon(Icons.check_circle_outline),
          label: const Text('Confirm arrival'),
        ),
        const SizedBox(height: 8),
        OutlinedButton.icon(
          onPressed: onCancel,
          icon: const Icon(Icons.cancel_outlined),
          label: const Text('Cancel order'),
        ),
      ],
    ],
  );
}

class _InfoRow extends StatelessWidget {
  const _InfoRow({
    required this.label,
    required this.value,
    this.emphasize = false,
  });
  final String label;
  final String value;
  final bool emphasize;
  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 6),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Expanded(
          child: Text(
            label,
            style: emphasize
                ? Theme.of(context).textTheme.titleMedium
                : Theme.of(context).textTheme.bodyMedium,
          ),
        ),
        const SizedBox(width: 12),
        Flexible(
          child: Text(
            value,
            textAlign: TextAlign.end,
            style: emphasize
                ? Theme.of(
                    context,
                  ).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold)
                : null,
          ),
        ),
      ],
    ),
  );
}

String _statusLabel(CustomerOrderStatus status) => switch (status) {
  CustomerOrderStatus.pending => 'Pending',
  CustomerOrderStatus.processing => 'Processing',
  CustomerOrderStatus.completed => 'Completed',
  CustomerOrderStatus.cancelled => 'Cancelled',
  CustomerOrderStatus.unknown => 'Unknown',
};

String _dateLabel(DateTime date) =>
    '${date.year}-${date.month.toString().padLeft(2, '0')}-${date.day.toString().padLeft(2, '0')}';
